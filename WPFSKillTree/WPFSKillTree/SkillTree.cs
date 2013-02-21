﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using POESKillTree.Parser;
using POESKillTree.Stats;
using Raven.Json.Linq;
using System.Diagnostics;

namespace POESKillTree
{
    public partial class SkillTree
    {
        public Stats.Character character;
        string TreeAddress = "http://www.pathofexile.com/passive-skill-tree/";
        public List<NodeGroup> NodeGroups = new List<NodeGroup>();
        public Dictionary<UInt16, SkillNode> Skillnodes = new Dictionary<UInt16, SkillNode>();
        public List<string> AttributeTypes = new List<string>();
        // public Bitmap iconActiveSkills;
        public SkillIcons iconInActiveSkills = new SkillIcons();
        public SkillIcons iconActiveSkills = new SkillIcons();
        public Dictionary<string, string> nodeBackgrounds = new Dictionary<string, string>() { { "normal", "PSSkillFrame" }, { "notable", "NotableFrameUnallocated" }, { "keystone", "KeystoneFrameUnallocated" } };
        public Dictionary<string, string> nodeBackgroundsActive = new Dictionary<string, string>() { { "normal", "PSSkillFrameActive" }, { "notable", "NotableFrameAllocated" }, { "keystone", "KeystoneFrameAllocated" } };
        public List<string> FaceNames = new List<string>() { "centermarauder", "centerranger", "centerwitch", "centerduelist", "centertemplar", "centershadow" };
        public List<string> CharName = new List<string>() { "MARAUDER", "RANGER", "WITCH", "DUELIST", "TEMPLAR", "SIX" };
        public Dictionary<string, float>[] CharBaseAttributes = new Dictionary<string, float>[6];
        public Dictionary<string, float> BaseAttributes = new Dictionary<string, float>()
                                                              {
                                                                  {"+# to maximum Mana",36},
                                                                  {"+# to maximum Life",45},
                                                                  {"Evasion Rating: #",50},
                                                                  {"+# Maximum Endurance Charge",3},
                                                                  {"+# Maximum Frenzy Charge",3},
                                                                  {"+# Maximum Power Charge",3},
                                                                  {"#% Additional Elemental Resistance per Endurance Charge",5},
                                                                  {"#% Physical Damage Reduction per Endurance Charge",5},
                                                                  {"#% Attack Speed Increase per Frenzy Charge",5},
                                                                  {"#% Cast Speed Increase per Frenzy Charge",5},
                                                                  {"#% Critical Strike Chance Increase per Power Charge",50},
                                                              };
        public static float LifePerLevel = 5;
        public static float EvasPerLevel = 3;
        public static float ManaPerLevel = 4;
        public static float IntPerMana = 2;
        public static float IntPerES = 5; //%
        public static float StrPerLife = 2;
        public static float StrPerED = 5; //%
        public static float DexPerAcc = 0.5f;
        public static float DexPerEvas = 5; //%
        private List<SkillTree.SkillNode> highlightnodes; 
        private int level = 1;
        private int chartype = 0;
        public HashSet<ushort> SkilledNodes = new HashSet<ushort>();
        public HashSet<ushort> AvailNodes = new HashSet<ushort>();
        Dictionary<string, Asset> assets = new Dictionary<string, Asset>();
        public Rect2D TRect = new Rect2D();
        public float scaleFactor = 1;
        public HashSet<int[]> Links = new HashSet<int[]>();
        public void Reset()
        {
            character = new Character( new CharacterBaseConfig( (CharacterType)chartype ) );
            SkilledNodes.Clear();
            var node = Skillnodes.First(nd => nd.Value.name == CharName[chartype]);
            SkilledNodes.Add(node.Value.id);
            UpdateAvailNodes();
        }
        public static SkillTree CreateSkillTree()
        {

            string skilltreeobj = "";
            if (Directory.Exists("Data"))
            {
                if (File.Exists("Data\\Skilltree.txt"))
                {
                    skilltreeobj = File.ReadAllText("Data\\Skilltree.txt");
                }
            }
            else
            {
                Directory.CreateDirectory("Data");
                Directory.CreateDirectory("Data\\Assets");
            }

            if (skilltreeobj == "")
            {

                string uriString = "http://www.pathofexile.com/passive-skill-tree/";
                WebRequest req = WebRequest.Create(uriString);

                WebResponse resp = req.GetResponse();
                string code = new StreamReader(resp.GetResponseStream()).ReadToEnd();
                Regex regex = new Regex("var passiveSkillTreeData.*");
                skilltreeobj = regex.Match(code).Value.Replace("root", "main").Replace("\\/", "/");
                skilltreeobj = skilltreeobj.Substring(27, skilltreeobj.Length - 27 - 2) + "";
                File.WriteAllText("Data\\Skilltree.txt", skilltreeobj);
            }

            return new SkillTree(skilltreeobj);
        }
        public SkillTree(String treestring)
        {

            RavenJObject jObject = RavenJObject.Parse(treestring.Replace("Additional ", ""));

            int qindex = 0;



            foreach (var obj in (((RavenJObject)jObject["skillSprites"])))
            {
                if (obj.Key.Contains("inactive"))continue;
                RavenJObject jobj = (RavenJObject)((RavenJArray)(obj.Value))[3];
                iconActiveSkills.Images[jobj["filename"].Value<string>()]= null;
                foreach (string s in ((RavenJObject)jobj["coords"]).Keys)
                {
                     RavenJObject o = (RavenJObject)(((RavenJObject)jobj["coords"])[s]);
                     iconActiveSkills.SkillPositions[s] = new KeyValuePair<Rect, string>(new Rect(o["x"].Value<int>(), o["y"].Value<int>(), o["w"].Value<int>(), o["h"].Value<int>()), jobj["filename"].Value<string>());
                }
                //iconActiveSkills.SkillPositions[qindex] = new Dictionary<string, KeyValuePair<Rect,string>>();
               // iconActiveSkills.Images.Add(jobj[0].Value<string>(),null);
               // iconActiveSkills.Images.Add(jobj["filename"].Value<string>(), null);
                //foreach (string s in ((RavenJObject)(jobj["coords"])).Keys)
                //{
                //    RavenJObject o = (RavenJObject)(((RavenJObject)jobj["coords"])[s]);
                //    //iconActiveSkills.SkillPositions[qindex][s] = new Rect(o["x"].Value<int>(), o["y"].Value<int>(),
                //                                                       //   o["w"].Value<int>(), o["h"].Value<int>());
                //}
                //foreach (string s in ((RavenJObject)(jobj["notableCoords"])).Keys)
                //{
                //    RavenJObject o = (RavenJObject)(((RavenJObject)jobj["notableCoords"])[s]);
                //    iconActiveSkills.SkillPositions[qindex][s] = new Rect(o["x"].Value<int>(), o["y"].Value<int>(),
                //                                                          o["w"].Value<int>(), o["h"].Value<int>());
                //}
                //foreach (string s in ((RavenJObject)(jobj["keystoneCoords"])).Keys)
                //{
                //    RavenJObject o = (RavenJObject)(((RavenJObject)jobj["keystoneCoords"])[s]);
                //    iconActiveSkills.SkillPositions[qindex][s] = new Rect(o["x"].Value<int>(), o["y"].Value<int>(),
                //                                                          o["w"].Value<int>(), o["h"].Value<int>());
                //}
                qindex++;
            }
            qindex = 0;
            foreach (var obj in (((RavenJObject)jObject["skillSprites"])))
            {
                if (obj.Key.Contains("active")) continue;
                RavenJObject jobj = (RavenJObject) ((RavenJArray) (obj.Value))[3];
                iconInActiveSkills.Images[jobj["filename"].Value<string>()] = null;
                foreach (string s in ((RavenJObject) jobj["coords"]).Keys)
                {
                    RavenJObject o = (RavenJObject) (((RavenJObject) jobj["coords"])[s]);
                    iconInActiveSkills.SkillPositions[s] =
                        new KeyValuePair<Rect, string>(new Rect(o["x"].Value<int>(), o["y"].Value<int>(), o["w"].Value<int>(), o["h"].Value<int>()), jobj["filename"].Value<string>());
                }
            }

            foreach (string s in ((RavenJObject)(jObject["assets"])).Keys)
            {
                if (((RavenJObject)((RavenJObject)(jObject["assets"]))[s])["0.3835"].Value<string>() == null)
                    continue;
                assets[s] = new Asset(s,
                                      ((RavenJObject)((RavenJObject)(jObject["assets"]))[s])["0.3835"].Value
                                          <string>());
            }
            iconActiveSkills.OpenOrDownloadImages();
            iconInActiveSkills.OpenOrDownloadImages();

            foreach (string s in ((RavenJObject)jObject["characterData"]).Keys)
            {
                var co = ((RavenJObject)((RavenJObject)jObject["characterData"])[s]);
                CharBaseAttributes[int.Parse(s) - 1] = new Dictionary<string, float>() { { "+# to Strength", co["base_str"].Value<int>() }, { "+# to Dexterity", co["base_dex"].Value<int>() }, { "+# to Intelligence", co["base_int"].Value<int>() } };
            }

            Parser.SkillNodeParser nodeParser = new SkillNodeParser();

            foreach (RavenJObject token in jObject["nodes"].Values())
            {
                Skillnodes.Add(token["id"].Value<ushort>(), new SkillTree.SkillNode()
                {
                    id = token["id"].Value<UInt16>(),
                    a = token["a"].Value<int>(),
                    name = token["dn"].Value<string>(),
                    attributes = token["sd"].Values<string>().ToArray(),
                    orbit = token["o"].Value<int>(),
                    orbitIndex = token["oidx"].Value<int>(),
                    icon = token["icon"].Value<string>(),
                    linkID = token["out"].Values<int>().ToList(),
                    g = token["g"].Value<int>(),
                    da = token["da"].Value<int>(),
                    ia = token["ia"].Value<int>(),
                    ks = token["ks"].Value<bool>(),
                    not = token["not"].Value<bool>(),
                    sa = token["sa"].Value<int>(),
                    Mastery = token["m"].Value<bool>(),
                    statModifiers = nodeParser.parseNode( token["dn"].Value<string>(), token["sd"].Values<string>().ToArray() )
                });
            }
            List<ushort[]> links = new List<ushort[]>();
            foreach (var skillNode in Skillnodes)
            {
                foreach (ushort i in skillNode.Value.linkID)
                {
                    if (
                        links.Count(
                            nd => (nd[0] == i && nd[1] == skillNode.Key) || nd[0] == skillNode.Key && nd[1] == i) ==
                        1)
                    {
                        continue;
                    }
                    links.Add(new ushort[] { skillNode.Key, i });
                }
            }
            foreach (ushort[] ints in links)
            {
                if (!Skillnodes[ints[0]].Neighbor.Contains(Skillnodes[ints[1]]))
                    Skillnodes[ints[0]].Neighbor.Add(Skillnodes[ints[1]]);
                if (!Skillnodes[ints[1]].Neighbor.Contains(Skillnodes[ints[0]]))
                    Skillnodes[ints[1]].Neighbor.Add(Skillnodes[ints[0]]);
            }
            // skillNode.Value.Neighbor.Add(Skillnodes[i]);
            foreach (RavenJObject token in jObject["groups"].Values())
            {
                NodeGroup ng = new NodeGroup();

                ng.Nodes = token["n"].Values<int>().ToList();
                if (!(token["oo"] is RavenJArray))
                    ng.OcpOrb = ((RavenJObject)token["oo"]).ToDictionary(k => k.Key,
                                                                          k =>
                                                                          k.Value.Value<bool>());
                if ((token["oo"] is RavenJArray))
                {
                    ng.OcpOrb.Add("0", true);
                }
                ng.Position = new Vector2D(token["x"].Value<float>(), token["y"].Value<float>());


                NodeGroups.Add(ng);
            }
            foreach (SkillTree.NodeGroup @group in NodeGroups)
            {
                foreach (ushort node in group.Nodes)
                {
                    Skillnodes[node].NodeGroup = group;
                }
            }

            TRect = new Rect2D(new Vector2D(jObject["min_x"].Value<float>() * 1.1, jObject["min_y"].Value<float>() * 1.1),
                               new Vector2D(jObject["max_x"].Value<float>() * 1.1, jObject["max_y"].Value<float>() * 1.1));
           

         

            InitNodeSurround();
            DrawNodeSurround();
            DrawNodeBaseSurround();
            DrawSkillIconLayer();
            DrawBackgroundLayer();
            InitFaceBrushesAndLayer();
            DrawLinkBackgroundLayer(links);
            InitOtherDynamicLayers();
            CreateCombineVisual();


            Regex regexAttrib = new Regex("[0-9]*\\.?[0-9]+");
            foreach (var skillNode in Skillnodes)
            {
                skillNode.Value.Attributes = new Dictionary<string, List<float>>();
                foreach (string s in skillNode.Value.attributes)
                {

                    List<float> values = new List<float>();

                    foreach (Match m in regexAttrib.Matches(s))
                    {

                        if (!AttributeTypes.Contains(regexAttrib.Replace(s, "#"))) AttributeTypes.Add(regexAttrib.Replace(s, "#"));
                        if (m.Value == "") values.Add(float.NaN);
                        else values.Add(float.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture));

                    }
                    string cs = (regexAttrib.Replace(s, "#"));
                   
                        skillNode.Value.Attributes[cs] = values;
                    
                 

                }
            }

           
        }
        public Dictionary<string, List<float>> ImplicitAttributes(Dictionary<string, List<float>> attribs)
        {
            Dictionary<string, List<float>> retval = new Dictionary<string, List<float>>();
            // +# to Strength", co["base_str"].Value<int>() }, { "+# to Dexterity", co["base_dex"].Value<int>() }, { "+# to Intelligence", co["base_int"].Value<int>() } };
            retval["+# to maximum Mana"] = new List<float>() { attribs["+# to Intelligence"][0] / IntPerMana + level * ManaPerLevel };
            retval["+#% Energy Shield"] = new List<float>() { attribs["+# to Intelligence"][0] / IntPerES };

            retval["+# to maximum Life"] = new List<float>() { attribs["+# to Strength"][0] / IntPerMana + level * LifePerLevel };
            retval["+#% increased Melee Physical Damage"] = new List<float>() { attribs["+# to Strength"][0] / StrPerED };

            retval["+# Accuracy Rating"] = new List<float>() { attribs["+# to Dexterity"][0] / DexPerAcc };
            retval["Evasion Rating: #"] = new List<float>() { level*EvasPerLevel};
            retval["#% increased Evasion Rating"] = new List<float>() { attribs["+# to Dexterity"][0] / DexPerEvas };
            return retval;
        }
        public int Level
        {
            get { return level; }
            set { level = value; }
        }
        public int Chartype
        {
            get { return chartype; }
            set
            {
                chartype = value;
                Reset();
                DrawFaces();
            }
        }
        public List<ushort> GetShortestPathTo(ushort targetNode)
        {
            if (SkilledNodes.Contains(targetNode)) return new List<ushort>();
            if (AvailNodes.Contains(targetNode)) return new List<ushort>() { targetNode };
            HashSet<ushort> visited = new HashSet<ushort>(SkilledNodes);
            Dictionary<int, int> distance = new Dictionary<int, int>();
            Dictionary<ushort, ushort> parent = new Dictionary<ushort, ushort>();
            Queue<ushort> newOnes = new Queue<ushort>();
            foreach (var node in SkilledNodes)
            {
                distance.Add(node, 0);
            }
            foreach (var node in AvailNodes)
            {
                newOnes.Enqueue(node);
                distance.Add(node, 1);
            }
            while (newOnes.Count > 0)
            {
                ushort newNode = newOnes.Dequeue();
                int dis = distance[newNode];
                visited.Add(newNode);
                foreach (var connection in Skillnodes[newNode].Neighbor.Select(nd => nd.id))
                {
                    if (visited.Contains(connection)) continue;
                    if (distance.ContainsKey(connection)) continue;
                    if (CharName.Contains(Skillnodes[connection].name)) continue;
                    if (Skillnodes[newNode].Mastery) continue;
                    distance.Add(connection, dis + 1);
                    newOnes.Enqueue(connection);

                    parent.Add(connection, newNode);

                    if (connection == targetNode) break;
                }
            }

            if (!distance.ContainsKey(targetNode)) return new List<ushort>();

            Stack<ushort> path = new Stack<ushort>();
            ushort curr = targetNode;
            path.Push(curr);
            while (parent.ContainsKey(curr))
            {
                path.Push(parent[curr]);
                curr = parent[curr];
            }

            List<ushort> result = new List<ushort>();
            while (path.Count > 0)
                result.Add(path.Pop());

            return result;
        }
        public HashSet<ushort> ForceRefundNodePreview(ushort nodeId)
        {
            if (!SkilledNodes.Remove(nodeId)) return new HashSet<ushort>();

            SkilledNodes.Remove(nodeId);

            HashSet<ushort> front = new HashSet<ushort>();
            front.Add(SkilledNodes.First());
            foreach (var i in Skillnodes[SkilledNodes.First()].Neighbor)
                if (SkilledNodes.Contains(i.id))
                    front.Add(i.id);

            HashSet<ushort> skilled_reachable = new HashSet<ushort>(front);
            while (front.Count > 0)
            {
                HashSet<ushort> newFront = new HashSet<ushort>();
                foreach (var i in front)
                    foreach (var j in Skillnodes[i].Neighbor.Select(nd => nd.id))
                        if (!skilled_reachable.Contains(j) && SkilledNodes.Contains(j))
                        {
                            newFront.Add(j);
                            skilled_reachable.Add(j);
                        }

                front = newFront;
            }

            HashSet<ushort> unreachable = new HashSet<ushort>(SkilledNodes);
            foreach (var i in skilled_reachable)
                unreachable.Remove(i);
            unreachable.Add(nodeId);

            SkilledNodes.Add(nodeId);

            return unreachable;
        }
        public void ForceRefundNode(ushort nodeId)
        {
            if (!SkilledNodes.Remove(nodeId)) throw new InvalidOperationException();

            //SkilledNodes.Remove(nodeId);

            character.resetStatModifiers();

            HashSet<ushort> front = new HashSet<ushort>();
            front.Add(SkilledNodes.First());
            foreach (var i in Skillnodes[SkilledNodes.First()].Neighbor)
                if (SkilledNodes.Contains(i.id))
                {
                    front.Add( i.id );
                    buyNode( i.id );
                }
            HashSet<ushort> skilled_reachable = new HashSet<ushort>(front);
            while (front.Count > 0)
            {
                HashSet<ushort> newFront = new HashSet<ushort>();
                foreach (var i in front)
                    foreach (var j in Skillnodes[i].Neighbor.Select(nd => nd.id))
                        if (!skilled_reachable.Contains(j) && SkilledNodes.Contains(j))
                        {
                            newFront.Add(j);
                            skilled_reachable.Add(j);
                            buyNode( j );
                        }

                front = newFront;
            }

            SkilledNodes = skilled_reachable;
            AvailNodes = new HashSet<ushort>();
            UpdateAvailNodes();
        }
        public void LoadFromURL(string url)
        {
            string s = url.Substring(TreeAddress.Length).Replace("-", "+").Replace("_", "/");
            byte[] decbuff = Convert.FromBase64String(s);
            var i = BitConverter.ToInt32(new byte[] { decbuff[3], decbuff[2], decbuff[1], decbuff[1] }, 0);
            var b = decbuff[4];
            var j = 0L; if (i > 0) j = decbuff[5];
            List<UInt16> nodes = new List<UInt16>();
            for (int k = 6; k < decbuff.Length; k += 2)
            {
                byte[] dbff = new byte[] { decbuff[k + 1], decbuff[k + 0] };
               if (Skillnodes.Keys.Contains(BitConverter.ToUInt16(dbff, 0)))
                    nodes.Add((BitConverter.ToUInt16(dbff, 0)));

            }
            Chartype = b - 1;
            SkilledNodes.Clear();
            SkillTree.SkillNode startnode = Skillnodes.First(nd => nd.Value.name == CharName[Chartype].ToUpper()).Value;
            SkilledNodes.Add(startnode.id);
            foreach (ushort node in nodes)
            {
                SkilledNodes.Add(node);
                buyNode( node );
            }
            UpdateAvailNodes();
        }
        public string SaveToURL()
        {
            byte[] b = new byte[(SkilledNodes.Count - 1) * 2 + 6];
            var b2 = BitConverter.GetBytes(2);
            b[0] = b2[3];
            b[1] = b2[2];
            b[2] = b2[1];
            b[3] = b2[0];
            b[4] = (byte)(Chartype + 1);
            b[5] = (byte)(0);
            int pos = 6;
            foreach (var inn in SkilledNodes)
            {
                if (CharName.Contains(Skillnodes[inn].name)) continue;
                byte[] dbff = BitConverter.GetBytes((Int16)inn);
                b[pos++] = dbff[1];
                b[pos++] = dbff[0];
            }
            return TreeAddress + Convert.ToBase64String(b).Replace("/", "_").Replace("+", "-");

        }
        public void UpdateAvailNodes()
        {
            AvailNodes.Clear();
            foreach (ushort inode in SkilledNodes)
            {
                SkillNode node = Skillnodes[inode];
                foreach (SkillNode skillNode in node.Neighbor)
                {
                    if (!CharName.Contains(skillNode.name) && !SkilledNodes.Contains(skillNode.id))
                        AvailNodes.Add(skillNode.id);
                }
            }
            //  picActiveLinks = new DrawingVisual();

            Pen pen2 = new Pen(Brushes.Yellow, 15f);

            using (DrawingContext dc = picActiveLinks.RenderOpen())
            {
                foreach (var n1 in SkilledNodes)
                {
                    foreach (var n2 in Skillnodes[n1].Neighbor)
                    {
                        if (SkilledNodes.Contains(n2.id))
                        {
                            DrawConnection(dc, pen2, n2, Skillnodes[n1]);
                        }
                    }
                }
            }
            // picActiveLinks.Clear();
            DrawNodeSurround();
        }
        public Dictionary<string, List<float>> SelectedAttributes
        {
            get
            {
                Dictionary<string, List<float>> temp = SelectedAttributesWithoutImplicit;

                foreach (var a in ImplicitAttributes(temp))
                {
                    if (!temp.ContainsKey(a.Key))
                        temp[a.Key] = new List<float>();
                    for (int i = 0; i < a.Value.Count; i++)
                    {

                        if (temp.ContainsKey(a.Key) && temp[a.Key].Count > i)
                            temp[a.Key][i] += a.Value[i];
                        else
                        {
                            temp[a.Key].Add(a.Value[i]);
                        }
                    }
                }
                return temp;
            }
        }
        public Dictionary<string, List<float>> SelectedAttributesWithoutImplicit
        {
            get
            {
                Dictionary<string, List<float>> temp = new Dictionary<string, List<float>>();
                foreach (var attr in CharBaseAttributes[Chartype])
                {
                    if (!temp.ContainsKey(attr.Key))
                        temp[attr.Key] = new List<float>();

                    if (temp.ContainsKey(attr.Key) && temp[attr.Key].Count > 0)
                        temp[attr.Key][0] += attr.Value;
                    else
                    {
                        temp[attr.Key].Add(attr.Value);
                    }
                }

                foreach (var attr in BaseAttributes)
                {
                    if (!temp.ContainsKey(attr.Key))
                        temp[attr.Key] = new List<float>();

                    if (temp.ContainsKey(attr.Key) && temp[attr.Key].Count > 0)
                        temp[attr.Key][0] += attr.Value;
                    else
                    {
                        temp[attr.Key].Add(attr.Value);
                    }
                }

                foreach (ushort inode in SkilledNodes)
                {
                    SkillNode node = Skillnodes[inode];
                    foreach (var attr in node.Attributes)
                    {
                        if (!temp.ContainsKey(attr.Key))
                            temp[attr.Key] = new List<float>();
                        for (int i = 0; i < attr.Value.Count; i++)
                        {

                            if (temp.ContainsKey(attr.Key) && temp[attr.Key].Count > i)
                                temp[attr.Key][i] += attr.Value[i];
                            else
                            {
                                temp[attr.Key].Add(attr.Value[i]);
                            }
                        }

                    }
                }
               
                return temp;
            }
        }
        public class SkillIcons
        {
            
            public enum IconType
            {
                normal,
                notable,
                keystone
            }

            public Dictionary<string, KeyValuePair<Rect, string>> SkillPositions = new Dictionary<string,KeyValuePair<Rect, string>>();
            public Dictionary<String,BitmapImage> Images = new Dictionary<string, BitmapImage>();
            public static string urlpath = "http://www.pathofexile.com/image/build-gen/passive-skill-sprite/";
            public void OpenOrDownloadImages()
            {
                foreach (var image in Images.Keys.ToArray())
                {
                        if (!File.Exists("Data\\Assets\\" + image))
                        {
                            System.Net.WebClient _WebClient = new System.Net.WebClient();
                            _WebClient.DownloadFile(urlpath + image, "Data\\Assets\\" + image);
                        }
                        Images[image] = new BitmapImage(new Uri("Data\\Assets\\" + image, UriKind.Relative));
                    
                }
            }
        }
        public class NodeGroup
        {
            public Vector2D Position;// "x": 1105.14,"y": -5295.31,
            public Dictionary<string, bool> OcpOrb = new Dictionary<string, bool>(); //  "oo": {"1": true},
            public List<int> Nodes = new List<int>();// "n": [-28194677,769796679,-1093139159]

        }
        public class SkillNode
        {
            static public float[] skillsPerOrbit = { 1, 6, 12, 12, 12 };
            static public float[] orbitRadii = { 0, 81.5f, 163, 326, 489 };
            public HashSet<int> Connections = new HashSet<int>();
            public bool skilled = false;
            public UInt16 id; // "id": -28194677,
            public string icon;// icon "icon": "Art/2DArt/SkillIcons/passives/tempint.png",
            public bool ks; //"ks": false,
            public bool not;   // not": false,
            public string name;//"dn": "Block Recovery",
            public int a;// "a": 3,
            public string[] attributes;// "sd": ["8% increased Block Recovery"],
            public Dictionary<string, List<float>> Attributes;
            // public List<string> AttributeNames;
            //public List<> AttributesValues;
            public int g;// "g": 1,
            public int orbit;//  "o": 1,
            public int orbitIndex;// "oidx": 3,
            public int sa;//s "sa": 0,
            public int da;// "da": 0,
            public int ia;//"ia": 0,
            public List<int> linkID = new List<int>();// "out": []
            public bool Mastery;
            public List<StatModifier> statModifiers;

            public List<SkillNode> Neighbor = new List<SkillNode>();
            public NodeGroup NodeGroup;
            public Vector2D Position
            {
                get
                {
                    double d = orbitRadii[this.orbit];
                    double b = (2 * Math.PI * this.orbitIndex / skillsPerOrbit[this.orbit]);
                    return (NodeGroup.Position - new Vector2D(d * Math.Sin(-b), d * Math.Cos(-b)));
                }
            }
            public double Arc
            {
                get { return (2 * Math.PI * this.orbitIndex / skillsPerOrbit[this.orbit]); }
            }
        }
        public class Asset
        {
            public string Name;
            public BitmapImage PImage;
            public string URL;
            public Asset(string name, string url)
            {
                Name = name;
                URL = url;
                if (!File.Exists("Data\\Assets\\" + Name + ".png"))
                {

                    System.Net.WebClient _WebClient = new System.Net.WebClient();
                    _WebClient.DownloadFile(URL, "Data\\Assets\\" + Name + ".png");



                }
                PImage = new BitmapImage(new Uri("Data\\Assets\\" + Name + ".png", UriKind.Relative));

            }

        }

        public void HighlightNodes(string search, bool useregex  )
        {
            if (search == "")
            {
                DrawHighlights(highlightnodes= new List<SkillTree.SkillNode>());
                highlightnodes = null;
                return;
            }

            if (useregex)
            {
                try
                {
                    List<SkillTree.SkillNode> nodes = highlightnodes = Skillnodes.Values.Where(nd => nd.attributes.Where(att => new Regex(search, RegexOptions.IgnoreCase).IsMatch(att)).Count() > 0 || new Regex(search, RegexOptions.IgnoreCase).IsMatch(nd.name) && !nd.Mastery).ToList();
                    DrawHighlights(highlightnodes);
                }
                catch (Exception)
                { }

            }
            else
            {
                highlightnodes = Skillnodes.Values.Where(nd => nd.attributes.Where(att => att.ToLower().Contains(search.ToLower())).Count() != 0 || nd.name.ToLower().Contains(search.ToLower())&& !nd.Mastery).ToList();
                
                DrawHighlights(highlightnodes);
            }
        }
        public void SkillAllHighligtedNodes()
        {
            if (highlightnodes == null) return;
            HashSet<int> nodes = new HashSet<int>();
            foreach (var nd in highlightnodes)
            {
                nodes.Add(nd.id);
            }
            SkillStep(nodes);

        }
        private HashSet<int> SkillStep(HashSet<int> hs)
        {
            List<List<ushort>> pathes = new List<List<ushort>>();
            foreach (var nd in highlightnodes)
            {
                pathes.Add(GetShortestPathTo(nd.id));


            }
            pathes.Sort((p1, p2) => p1.Count.CompareTo(p2.Count));
            pathes.RemoveAll(p => p.Count == 0);
            foreach (ushort i in pathes[0])
            {
                hs.Remove(i);
                SkilledNodes.Add(i);
            }
            UpdateAvailNodes();

            return hs.Count == 0 ? hs : SkillStep(hs);
        }

        public void buyNode( ushort id )
        {
            SkillNode node = Skillnodes[ id ];
            if (node != null)
            {
                foreach (StatModifier modifier in node.statModifiers)
                {
                    character.addStatModifier( modifier );
                }
            }
        }

        public void setCharacterItems( List<Item> items )
        {
            foreach (Item item in items)
            {
                character.addItem( item );
            }
        }
    }


}
