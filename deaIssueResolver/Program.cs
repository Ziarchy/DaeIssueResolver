using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Linq;

namespace deaIssueResolver
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string dir = System.IO.Directory.GetCurrentDirectory();
            string inputDir = dir + "input/";
            string outputDir = dir + "\\output\\";
            string[] files = System.IO.Directory.GetFiles("input/", "*.dae");//allow some sort of manual selection

            XNamespace ns = "http://www.collada.org/2005/11/COLLADASchema";//need to get this automatically
            XDocument doc = XDocument.Load(files[0]);
            string curFile = files[0];
            IEnumerable<XElement> node = doc.Descendants(ns + "library_geometries");


            if (node.Count() == 0)
            {
                throw new Exception("no library for geometry found");
            }

            if (node.Count() > 1)
            {
                throw new Exception("too many geometry libraries found");
            }

            XContainer libGeo = node.First();
            List<XElement> allGeos = libGeo.Descendants(ns + "geometry").ToList();
            int meshCount = 0;
            int texCount = 0;
            for(int geos = allGeos.Count() - 1; geos >= 0; geos--)// find all geometry nodes and check if the have too many UV maps
            {
                meshCount = 0;
                texCount = 0;


                meshCount = allGeos[geos].Descendants(ns + "mesh").Count();
                if(meshCount > 1)//TODO maybe make it so it can handle this
                {
                    throw new Exception("more than1 mesh per geometry");
                }

                XContainer mesh = allGeos[geos].Descendants(ns + "mesh").First();
                if( mesh == null )
                {
                    continue;
                }
                IEnumerable<XElement> texInputs = mesh.Descendants(ns + "input").Where(x => x.Attribute("semantic").Value == "TEXCOORD");

                if(texInputs.Count() > 1)
                {
                    List<XElement> newGeos = new List<XElement>();
                    for(int i = 0; i < texInputs.Count(); i++)//create new geometry node for each UV
                    {
                        XElement copyGeo = new XElement((XElement)allGeos[geos]);
                        XElement copyMesh = copyGeo.Descendants(ns + "mesh").FirstOrDefault();
                        int firstTex = -1;

                        List<int> toRemove = new List<int>();

                        List<XElement> copyInputs = copyMesh.Descendants(ns + "triangles").FirstOrDefault().Descendants(ns + "input").ToList();//todo, fix this, work out to find triangle or polies, also this just sucks
                        int inputCount = copyInputs.Count();

                        for(int j = 0; j < copyInputs.Count(); j++) //work out what needs to be removed from the new copy
                        {
                            if (copyInputs.ElementAt(j).Attribute("semantic").Value == "TEXCOORD")
                            {
                                if (firstTex == -1)
                                {
                                    firstTex = j;
                                }
                                else
                                {
                                    toRemove.Add(j);
                                }
                            }
                        }
                        string pString = copyGeo.Descendants(ns + "p").FirstOrDefault().Value; //this is bad, need to get this properly
                        List<string> pList = pString.Split(" ").ToList();

                        int removeCounter;
                        for(int j = pList.Count - 1; j >= 0; j--)//removing inputs from p 
                        {
                            removeCounter = j % inputCount;
                            if (toRemove.Contains(removeCounter))
                            {
                                pList.RemoveAt(j);
                            }

                        }
                        string newP = string.Join(" ", pList.ToArray());
                        copyGeo.Descendants(ns + "p").FirstOrDefault().Value = newP;

                        var sources = copyGeo.Descendants(ns + "source").ToList();
                        for (int pos = toRemove.Count - 1; pos >= 0; pos--) //remove sources and inputs
                        {
                            int set = toRemove[pos];
                            sources.ElementAt(set).Remove();
                            copyInputs.ElementAt(set).Remove();
                        }
                        copyGeo.Attribute("name").Value = copyGeo.Attribute("name").Value + i;
                        newGeos.Add(copyGeo);

                        Console.WriteLine(copyGeo.ToString());
                    }
                    //remove original geometry and add new copies
                    allGeos[geos].AddAfterSelf(newGeos);
                    allGeos[geos].Remove();
                    
                }
                

            }
            doc.Save(outputDir + "Lft_BaseCampTrain_Octa.dae");
        }


        private List<string> ParseStringToList(string str, string delimiter)
        {
            List<string> result = str.Split(delimiter).ToList();
            return result;
        }

        private XmlDocument GetXmlDocument(string name)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("input/" + name);
            return doc;
        }

        //should take in the geometry to be duped and teh number of the texcoord that will be saved
        //the number will be the place of the texcoord in the full list of sources

        private XElement createGeoCopy(XElement original, int toKeep)
        {
            XElement copy = new XElement(original);
            IEnumerable<XElement> sources = copy.Descendants("source");
            IEnumerable<XElement> inputs = copy.Descendants("input");
            List<XElement> sourceToDelete = new List<XElement>();
            List<XElement> inputToDelete = new List<XElement>();
            List<bool> toSkip = new List<bool>();

            for (int i = 0; i < inputs.Count(); i++)
            {
                if (inputs.ElementAt(i).Attribute("semantic").Value == "TEXCOORD")
                {
                    if(i  == toKeep)
                    {
                        toSkip.Add(true);
                        continue;
                    }
                    else
                    {
                        sourceToDelete.Add(sources.ElementAt(i));
                        inputToDelete.Add(inputs.ElementAt(i));
                        toSkip.Add(false);
                    }
                    toSkip.Add(true);
                }
            }

            XElement triangles = copy.Descendants("triangles").FirstOrDefault();

            if(triangles == null)
            {
                throw new Exception("need to add the different types of whatever this is called");
                //this will likely be a new function that searches for all the different names and returns the one that is found
            }


            return copy;
        }
    }
}