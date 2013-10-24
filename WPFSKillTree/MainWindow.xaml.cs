﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using POESKillTree;
using WPFSKillTree;

namespace POESKillTree
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<PoEBuild> savedBuilds = new List<PoEBuild>( );

        private ItemAttributes ItemAttributes = null;
        SkillTree Tree;
        ToolTip sToolTip = new ToolTip( );
        private string lasttooltip;
        private Vector2D multransform = new Vector2D( );
        private Vector2D addtransform = new Vector2D( );
        public MainWindow( )
        {

            Thread.CurrentThread.SetApartmentState( ApartmentState.STA );
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            //AppDomain.CurrentDomain.AssemblyResolve += ( sender, args ) =>
            //{

            //    String resourceName = "POESKillTree." +

            //       new AssemblyName( args.Name ).Name + ".dll";

            //    using ( var stream = Assembly.GetExecutingAssembly( ).GetManifestResourceStream( resourceName ) )
            //    {

            //        Byte[] assemblyData = new Byte[ stream.Length ];

            //        stream.Read( assemblyData, 0, assemblyData.Length );

            //        return Assembly.Load( assemblyData );

            //    }

            //};

            InitializeComponent( );

        }
        static Action emptyDelegate = delegate
        {
        };

        private LoadingWindow loadingWindow;
        private void startLoadingWindow()
        {
            loadingWindow = new LoadingWindow();
             loadingWindow.Show();
        }
        private void updatetLoadingWindow(double c, double max )
        {
            loadingWindow.progressBar1.Maximum = max;
            loadingWindow.progressBar1.Value = c;
            loadingWindow.Dispatcher.Invoke( DispatcherPriority.Render , emptyDelegate );
        }
        private void closeLoadingWindow( )
        {

            loadingWindow.Close( );
        }

        private void border1_MouseMove( object sender , MouseEventArgs e )
        {
            Point p = e.GetPosition( border1.Child );
            Vector2D v = new Vector2D( p.X , p.Y );
            v = v * multransform + addtransform;
            textBox1.Text = "" + v.X;
            textBox2.Text = "" + v.Y;
            SkillTree.SkillNode node = null;

            var nodes = Tree.Skillnodes.Where( n => ( ( n.Value.Position - v ).Length < 50 ) );
            if ( nodes != null && nodes.Count( ) != 0 )
                node = nodes.First( ).Value;

            if ( node != null && node.Attributes.Count != 0 )
            {

                string tooltip = node.name + "\n" + node.attributes.Aggregate( ( s1 , s2 ) => s1 + "\n" + s2 );
                if ( !( sToolTip.IsOpen == true && lasttooltip == tooltip ) )
                {
                    sToolTip.Content = tooltip;
                    sToolTip.IsOpen = true;
                    lasttooltip = tooltip;
                }
                if ( Tree.SkilledNodes.Contains( node.id ) )
                {
                    toRemove = Tree.ForceRefundNodePreview( node.id );
                    if ( toRemove != null )
                        Tree.DrawRefundPreview( toRemove );
                }
                else
                {
                    prePath = Tree.GetShortestPathTo( node.id );
                    Tree.DrawPath( prePath );
                }

            }
            else
            {
                sToolTip.Tag = false;
                sToolTip.IsOpen = false;
                prePath = null;
                toRemove = null;
                if ( Tree != null )
                {
                    Tree.ClearPath( );
                }

            }

        }
        private List<ushort> prePath;
        private HashSet<ushort> toRemove;
        private bool justLoaded = false;
        private void comboBox1_SelectionChanged( object sender , SelectionChangedEventArgs e )
        {
            if ( justLoaded )
            {
                justLoaded = false;
                return;
            }

            if ( Tree == null )
                return;
            SkillTree.SkillNode startnode = Tree.Skillnodes.First( nd => nd.Value.name.ToUpper() == ( Tree.CharName[ cbCharType.SelectedIndex ] ).ToUpper( ) ).Value;
            Tree.SkilledNodes.Clear( );
            Tree.SkilledNodes.Add( startnode.id );
            Tree.Chartype = Tree.CharName.IndexOf( ( Tree.CharName[ cbCharType.SelectedIndex ] ).ToUpper( ) );
            Tree.UpdateAvailNodes( );
            UpdateAllAttributeList( );
        }
        private void Window_SizeChanged( object sender , SizeChangedEventArgs e )
        {

        }
        private void border1_Click( object sender , RoutedEventArgs e )
        {

            Point p = ( ( MouseEventArgs )e.OriginalSource ).GetPosition( border1.Child );
            Vector2D v = new Vector2D( p.X , p.Y );

            v = v * multransform + addtransform;
            SkillTree.SkillNode node = null;

            var nodes = Tree.Skillnodes.Where( n => ( ( n.Value.Position - v ).Length < 50 ) );
            if ( nodes != null && nodes.Count( ) != 0 )
            {
                node = nodes.First( ).Value;

                if ( Tree.SkilledNodes.Contains( node.id ) )
                {
                    Tree.ForceRefundNode( node.id );
                    UpdateAllAttributeList( );

                    prePath = Tree.GetShortestPathTo( node.id );
                    Tree.DrawPath( prePath );
                }
                else if ( prePath != null )
                {
                    foreach ( ushort i in prePath )
                    {
                        Tree.SkilledNodes.Add( i );
                    }
                    UpdateAllAttributeList( );
                    Tree.UpdateAvailNodes( );

                    toRemove = Tree.ForceRefundNodePreview( node.id );
                    if ( toRemove != null )
                        Tree.DrawRefundPreview( toRemove );
                }
            }
            tbSkillURL.Text = Tree.SaveToURL( );
        }
        private List<string> attiblist = new List<string>( );
        private ListCollectionView AttibuteCollection;
        Regex backreplace = new Regex( "#" );
        private string InsertNumbersInAttributes( KeyValuePair<string , List<float>> attrib )
        {
            string s = attrib.Key;
            foreach ( var f in attrib.Value )
            {
                s = backreplace.Replace( s , f + "" , 1 );
            }
            return s;
        }
        public void UpdateAttributeList( )
        {

            attiblist.Clear( );
            foreach ( var item in ( Tree.SelectedAttributes.Select( InsertNumbersInAttributes ) ) )
            {
                attiblist.Add( item );

            }
            AttibuteCollection.Refresh( );
            tbUsedPoints.Text = "" + ( Tree.SkilledNodes.Count - 1 );
        }
        private List<string> allAttributesList = new List<string>( );
        private ListCollectionView AllAttributeCollection;
        public void UpdateAllAttributeList( )
        {
            if ( ItemAttributes != null )
            {


                var attritemp = Tree.SelectedAttributesWithoutImplicit;
                foreach ( ItemAttributes.Attribute mod in ItemAttributes.NonLocalMods )
                {
                    if ( attritemp.ContainsKey( mod.TextAttribute ) )
                    {
                        for ( int i = 0 ; i < mod.Value.Count ; i++ )
                        {
                            attritemp[ mod.TextAttribute ][ i ] += mod.Value[ i ];
                        }
                    }
                    else
                    {
                        attritemp[ mod.TextAttribute ] = mod.Value;
                    }
                }

                foreach ( var a in Tree.ImplicitAttributes( attritemp ) )
                {
                    if ( !attritemp.ContainsKey( a.Key ) )
                        attritemp[ a.Key ] = new List<float>( );
                    for ( int i = 0 ; i < a.Value.Count ; i++ )
                    {

                        if ( attritemp.ContainsKey( a.Key ) && attritemp[ a.Key ].Count > i )
                            attritemp[ a.Key ][ i ] += a.Value[ i ];
                        else
                        {
                            attritemp[ a.Key ].Add( a.Value[ i ] );
                        }
                    }
                }

                allAttributesList.Clear( );
                foreach ( var item in ( attritemp.Select( InsertNumbersInAttributes ) ) )
                {
                    allAttributesList.Add( item );

                }
                AllAttributeCollection.Refresh( );
            }

            UpdateAttributeList( );
        }
        string TreeAddress = "http://www.pathofexile.com/passive-skill-tree/";
        private void button2_Click( object sender , RoutedEventArgs e )
        {
            try
            {
                if (tbSkillURL.Text.Contains("poezone.ru"))
                {
                    SkillTreeImporter.LoadBuildFromPoezone(Tree, tbSkillURL.Text);
                    tbSkillURL.Text = Tree.SaveToURL();
                }
                else
                    Tree.LoadFromURL(tbSkillURL.Text);

                justLoaded = true;
                cbCharType.SelectedIndex = Tree.Chartype;
                UpdateAllAttributeList();
            }
            catch ( Exception )
            {
                MessageBox.Show("The Build you tried to load, is invalid");
            }
        }
        [ValueConversion( typeof( string ) , typeof( string ) )]
        public class GroupStringConverter : IValueConverter
        {
            static GroupStringConverter( )
            {
                if ( !File.Exists( "groups.txt" ) )
                    return;
                Groups.Clear( );
                foreach ( string s in File.ReadAllLines( "groups.txt" ) )
                {
                    string[] sa = s.Split( ',' );
                    Groups.Add( sa );
                }
            }
            public static List<string[]> Groups = new List<string[]>( )
                                                                 { 
                                                                     new []{"charg","Charge"},
                                                                     new []{"weapon","Weapon"},
                                                                       new []{"melee phys","Weapon"},
                                                                       new []{"physical dam","Weapon"},
                                                                       new []{"area","Spell"},
                                                                       new []{"crit","Crit"},
                                                                       new []{"pierc","Weapon"},
                                                                       new []{"proj","Weapon"},
                                                                       new []{"minio","Minion"},
                                                                       new []{"move","Defense"},
                                                                       new []{"mana","Spell"},
                                                                       new []{"life","Defense"},
                                                                       new []{"armour","Defense"},
                                                                       new []{"evasi","Defense"},
                                                                       new []{"defence","Defense"},
                                                                       new []{"buff","Spell"},
                                                                       new []{"spell","Spell"},
                                                                       new []{"cast","Spell"},
                                                                       new []{"attack","Weapon"},
                                                                       new []{"accur","Weapon"},
                                                                       new []{"intel","BaseStats"},
                                                                       new []{"dex","BaseStats"},
                                                                       new []{"stre","BaseStats"},
                                                                       new []{"shield","Defense"},
                                                                       new []{"dual wiel","Weapon"},
                                                                       new []{"bow","Weapon"},
                                                                       new []{"axe","Weapon"},
                                                                       new []{"mace","Weapon"},
                                                                       new []{"stav","Weapon"},
                                                                       new []{"staff","Weapon"},
                                                                       new []{"dagg","Weapon"},
                                                                       new []{"claw","Weapon"},
                                                                       new []{"wand","Weapon"},
                                                                       new []{"zombie","Minion"},
                                                                       new []{"spectre","Minion"},
                                                                       new []{"all attrib","BaseStats"},
                                                                       new []{"resist","Defense"},

                                                                 };
            public object Convert( object value , Type targetType , object parameter , CultureInfo culture )
            {
                string s = ( string )value;
                foreach ( var gp in Groups )
                {
                    if ( s.ToLower( ).Contains( gp[ 0 ].ToLower( ) ) )
                    {
                        return gp[ 1 ];
                    }
                }
                return "Everything else";
            }

            public object ConvertBack( object value , Type targetType , object parameter , CultureInfo culture )
            {
                throw new NotImplementedException( );
            }
        }
        public class NumberLessStringComparer : IComparer<string>
        {
            static Regex numberfilter = new Regex( @"[0-9\\.]+" );

            public int Compare( string x , string y )
            {
                return numberfilter.Replace( x , "" ).CompareTo( numberfilter.Replace( y , "" ) );
            }
        }
        private void Window_Closing( object sender , System.ComponentModel.CancelEventArgs e )
        {
            File.WriteAllText( "skilltreeAddress.txt" , tbSkillURL.Text + "\n" + tbLevel.Text );

            if ( lvSavedBuilds.Items.Count > 0 )
            {
                StringBuilder rawBuilds = new StringBuilder( );
                foreach ( ListViewItem lvi in lvSavedBuilds.Items )
                {
                    PoEBuild build = ( PoEBuild )lvi.Content;
                    rawBuilds.Append( build.name + '|' + build.description + ';' + build.url + '\n' );
                }
                File.WriteAllText( "savedBuilds" , rawBuilds.ToString( ).Trim( ) );
            }
            else
            {
                if ( File.Exists( "savedBuilds" ) )
                {
                    File.Delete( "savedBuilds" );
                }
            }
        }
        private void tbSkillURL_MouseDoubleClick( object sender , MouseButtonEventArgs e )
        {
            tbSkillURL.SelectAll( );
        }
        private void button1_Click_1( object sender , RoutedEventArgs e )
        {
            string filetoload = "";
            if (File.Exists( "Data\\get-items" ))
            {
                filetoload = "Data\\get-items";
            }
            else if (File.Exists( "Data\\get-items.txt" ))
            {
                filetoload = "Data\\get-items.txt";
            }
            else
            {
                popup1.IsOpen = true;
                return;
            }

         
            ItemAttributes = new ItemAttributes( filetoload );
            lbItemAttr.ItemsSource = ItemAttributes.Attributes;
            UpdateAllAttributeList( );
     



        }
        private void btnPopup_OnClick( object sender , RoutedEventArgs e )
        {
            popup1.IsOpen = false;
        }
        private void btnDownloadItemData_Click( object sender , RoutedEventArgs e )
        {
            popup1.IsOpen = false;
            System.Diagnostics.Process.Start( "http://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text );
        }
        private void tbCharName_TextChanged( object sender , TextChangedEventArgs e )
        {
            tbCharLink.Text = "http://www.pathofexile.com/character-window/get-items?character=" + tbCharName.Text;
        }
        private void tbSearch_TextChanged( object sender , TextChangedEventArgs e )
        {
            Tree.HighlightNodes( tbSearch.Text , checkBox1.IsChecked.Value );
        }
        private void textBox3_TextChanged( object sender , TextChangedEventArgs e )
        {
            int lvl = 0;
            if ( int.TryParse( tbLevel.Text , out lvl ) )
            {
                Tree.Level = lvl;
                UpdateAllAttributeList( );
            }
        }
        private void button3_Click( object sender , RoutedEventArgs e )
        {
            Tree.SkillAllHighligtedNodes( );
            UpdateAllAttributeList( );
        }
        private void button4_Click( object sender , RoutedEventArgs e )
        {
            if ( Tree == null )
                return;
            Tree.Reset( );

            UpdateAllAttributeList( );
        }
        private RenderTargetBitmap ClipboardBmp;
        private void btnScreenShot_Click( object sender , RoutedEventArgs e )
        {
            int maxsize = 3000;
            Geometry geometry = Tree.picActiveLinks.Clip;
            Rect2D contentBounds = Tree.picActiveLinks.ContentBounds;
            contentBounds *= 1.2;


            double aspect = contentBounds.Width / contentBounds.Height;
            double xmax = contentBounds.Width;
            double ymax = contentBounds.Height;
            if ( aspect > 1 && xmax > maxsize )
            {
                xmax = maxsize;
                ymax = xmax / aspect;
            }
            if ( aspect < 1 & ymax > maxsize )
            {
                ymax = maxsize;
                xmax = ymax * aspect;
            }

            ClipboardBmp = new RenderTargetBitmap( ( int )xmax , ( int )ymax , 96 , 96 , PixelFormats.Pbgra32 );
            VisualBrush db = new VisualBrush( Tree.SkillTreeVisual );
            db.ViewboxUnits = BrushMappingMode.Absolute;
            db.Viewbox = contentBounds;
            DrawingVisual dw = new DrawingVisual( );

            using ( DrawingContext dc = dw.RenderOpen( ) )
            {
                dc.DrawRectangle( db , null , new Rect( 0 , 0 , xmax , ymax ) );
            }
            ClipboardBmp.Render( dw );
            ClipboardBmp.Freeze( );

            Clipboard.SetImage( ClipboardBmp );

            image1.Fill = new VisualBrush( Tree.SkillTreeVisual );

        }

        private void Window_Loaded( object sender , RoutedEventArgs e )
        {
            AttibuteCollection = new ListCollectionView( attiblist );

            listBox1.ItemsSource = AttibuteCollection;
            // AttibuteCollection.CustomSort = 
            PropertyGroupDescription pgd = new PropertyGroupDescription( "" );
            pgd.Converter = new GroupStringConverter( );
            AttibuteCollection.GroupDescriptions.Add( pgd );

            AllAttributeCollection = new ListCollectionView( allAttributesList );
            AllAttributeCollection.GroupDescriptions.Add( pgd );
            lbAllAttr.ItemsSource = AllAttributeCollection;

            Tree = SkillTree.CreateSkillTree(startLoadingWindow,updatetLoadingWindow,closeLoadingWindow );
            image1.Fill = new VisualBrush( Tree.SkillTreeVisual );


            Tree.Chartype = Tree.CharName.IndexOf( ( ( string )( ( ComboBoxItem )cbCharType.SelectedItem ).Content ).ToUpper( ) );
            Tree.UpdateAvailNodes( );
            UpdateAllAttributeList( );

            multransform = Tree.TRect.Size / image1.RenderSize.Height;
            addtransform = Tree.TRect.TopLeft;

            // loading last build
            if ( File.Exists( "skilltreeAddress.txt" ) )
            {
                string s = File.ReadAllText( "skilltreeAddress.txt" );
                tbSkillURL.Text = s.Split( '\n' )[ 0 ];
                tbLevel.Text = s.Split( '\n' )[ 1 ];
                button2_Click( this , new RoutedEventArgs( ) );
                justLoaded = false;
            }

            // loading saved build
            try
            {
                if ( File.Exists( "savedBuilds" ) )
                {
                    string[] builds = File.ReadAllText( "savedBuilds" ).Split( '\n' );
                    foreach ( string b in builds )
                    {
                        savedBuilds.Add( new PoEBuild( b.Split( ';' )[ 0 ].Split( '|' )[ 0 ] , b.Split( ';' )[ 0 ].Split( '|' )[ 1 ] , b.Split( ';' )[ 1 ] ) );
                    }

                    lvSavedBuilds.Items.Clear( );
                    foreach ( PoEBuild build in savedBuilds )
                    {
                        ListViewItem lvi = new ListViewItem
                        {
                            Content = build
                        };
                        lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                        lvSavedBuilds.Items.Add( lvi );
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show( "Unable to load the saved builds." , "Error" , MessageBoxButton.OK , MessageBoxImage.Error );
            }
        }

        void lvi_MouseDoubleClick( object sender , MouseButtonEventArgs e )
        {
            ListViewItem lvi = ( ListViewItem )sender;
            tbSkillURL.Text = ( ( PoEBuild )lvi.Content ).url;
            button2_Click( this , null ); // loading the build
        }

        private void btnSaveNewBuild_Click( object sender , RoutedEventArgs e )
        {
            FormBuildName formBuildName = new FormBuildName( );
            if ( ( bool )formBuildName.ShowDialog( ) )
            {
                ListViewItem lvi = new ListViewItem
                {
                    Content = new PoEBuild( formBuildName.getBuildName( ) , cbCharType.Text + ", " + tbUsedPoints.Text + " points used" , tbSkillURL.Text )
                };
                lvi.MouseDoubleClick += lvi_MouseDoubleClick;
                lvSavedBuilds.Items.Add( lvi );
            }
        }

        private void btnDelete_Click( object sender , RoutedEventArgs e )
        {
            if ( lvSavedBuilds.SelectedItems.Count > 0 )
            {
                lvSavedBuilds.Items.Remove( lvSavedBuilds.SelectedItem );
            }
        }

        private void btnOverwriteBuild_Click( object sender , RoutedEventArgs e )
        {
            if ( lvSavedBuilds.SelectedItems.Count > 0 )
            {
                ( ( ListViewItem )lvSavedBuilds.SelectedItem ).Content = new PoEBuild( ( ( ListViewItem )lvSavedBuilds.SelectedItem ).Content.ToString( ).Split( '\n' )[ 0 ] , cbCharType.Text + ", " + tbUsedPoints.Text + " points used" , tbSkillURL.Text );
            }
            else
            {
                MessageBox.Show( "Please select an existing build first." , "Error" , MessageBoxButton.OK , MessageBoxImage.Error );
            }
        }

        private void btnDownloadItemData_Copy_Click( object sender , RoutedEventArgs e )
        {
            popup1.IsOpen = false;
            var fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
           bool? ftoload= fileDialog.ShowDialog(this);
            if (ftoload.Value)
            {
            ItemAttributes = new ItemAttributes( fileDialog.FileName );
            lbItemAttr.ItemsSource = ItemAttributes.Attributes;
            UpdateAllAttributeList( );
            }


        }

        private void btnCopyStats_Click( object sender , RoutedEventArgs e )
        {
            StringBuilder sb = new StringBuilder();
            foreach (var at in attiblist)
            {
                sb.AppendLine(at);
            }
            Clipboard.SetText(sb.ToString(),TextDataFormat.Text);
           
        }
    }

    class PoEBuild
    {
        public string name, description, url;
        public PoEBuild( string n , string d , string u )
        {
            this.name = n;
            this.description = d;
            this.url = u;
        }
        public override string ToString( )
        {
            return name + '\n' + description;
        }
    }
}

