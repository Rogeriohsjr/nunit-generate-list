using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace nunit_generate_list
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Converting Visual Studio Playlist or NUnit Report file to NUnit File(.tst).");

            try
            {
            if (args.Length != 2)
                {
                    ShowMessage("We need two parameters source file and destination file.", "E");
                    ShowMessage("Example: nunit-generate-list.exe \"C:\\MyTests.playlist\" \"C:\\ListTestFromVSPlaylist.tst\" ", "E");
                    return;
            }

string pathFrom = args[0];
                string pathTo = args[1];

                Program obj = new Program();
                List<string> objListTest = new List<string>();

                if (pathFrom.ToLower().Contains(".playlist"))
                {
                    Console.WriteLine("Converting Playlist file to NUnit File.");
                    Playlist objPlayList = obj.GetPlayList(pathFrom);
                    objListTest = obj.GetListTestForNUnitFile(objPlayList);
                    obj.CreateNUnitTestFile(pathTo, objListTest);

                    ShowMessage("Converted Successfully!");
                }
                else if (pathFrom.ToLower().Contains(".xml"))
                {
                    Console.WriteLine("Converting NUnit Report file to NUnit File.");
                    NUnitReport objUnitReport = obj.GetNUnitResult(pathFrom);
                    objListTest = obj.GetListTestForNUnitFile(objUnitReport);
                    obj.CreateNUnitTestFile(pathTo, objListTest);

                    ShowMessage("Converted Successfully!");
                }
                else
                {
                    ShowMessage("We didn't find .playlist or .xml in the first path.", "E");
                    ShowMessage("Example: nunit-generate-list.exe \"C:\\MyTests.playlist\" \"C:\\ListTestFromVSPlaylist.tst\" ", "E");
                }
            }
            catch (Exception ex)
            {
                ShowMessage("Error unexpected! Expection[" + ex + "]", "E");
            }
        }

        public static void ShowMessage(string pMessage, string pType = "S")
        {
            ConsoleColor oldColor = Console.ForegroundColor;
            ConsoleColor messageColor = oldColor;
            switch (pType)
            {
                case "S":
                    messageColor = ConsoleColor.Green;
                    break;
                case "E":
                    messageColor = ConsoleColor.Red;
                    break;
                default:
                    break;
            }

            Console.ForegroundColor = messageColor;
            Console.WriteLine(pMessage);
            Console.ForegroundColor = oldColor;
        }

        public NUnitReport GetNUnitResult(string pPath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(pPath);

            XmlNodeList objListTestCases = xmlDocument.GetElementsByTagName("test-case");

            NUnitReport objNUnitReport = new NUnitReport();
            foreach (XmlNode fTestCase in objListTestCases)
            {
                TestCases objTestCase = new TestCases
                {
                    FullName = fTestCase.Attributes["name"]?.Value.ToString(),
                    Result = fTestCase.Attributes["result"]?.Value.ToString()
                };

                objNUnitReport.ListTestCases.Add(objTestCase);
            }

            return objNUnitReport;
        }

        public List<string> GetListTestForNUnitFile(NUnitReport pNUnitReport)
        {
            List<string> listTest = new List<string>();
            foreach (TestCases fTestCase in pNUnitReport.ListTestCases)
            {
                if (fTestCase.Result.ToLower() == "Error".ToLower() || fTestCase.Result.ToLower() == "Failed".ToLower())
                {
                    listTest.Add(fTestCase.FullName);
                }
            }

            return listTest;
        }

        public Playlist GetPlayList(string pPath)
        {
            Playlist objPlaylist = new Playlist();

            XmlSerializer objXMLSerializer = new XmlSerializer(typeof(Playlist));
            using (XmlReader reader = XmlReader.Create(pPath))
            {
                objPlaylist = (Playlist)objXMLSerializer.Deserialize(reader);
            }

            return objPlaylist;
        }

        public List<string> GetListTestForNUnitFile(Playlist pPlayList)
        {
            List<string> listTest = new List<string>();
            foreach (Add fTest in pPlayList.Add)
            {
                listTest.Add(fTest.Test);
            }

            return listTest;
        }

        public void CreateNUnitTestFile(string pPath, List<string> pListTests)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(pPath))
            {
                foreach (string fTestName in pListTests)
                {
                    file.WriteLine(fTestName);
                }
            }
        }

        [Serializable]
        public class Playlist
        {
            public Playlist()
            {
            }

            [XmlElement("Add")]
            public Add[] Add { get; set; }
        }

        [Serializable]
        public class Add
        {
            public Add()
            {
            }

            [XmlAttribute]
            public string Test { get; set; }
        }

        public class NUnitReport
        {
            public NUnitReport()
            {
                this.ListTestCases = new List<TestCases>();
            }

            public List<TestCases> ListTestCases { get; set; }
        }

        public class TestCases
        {
            public TestCases()
            {
                this.FullName = string.Empty;
                this.Result = string.Empty;
            }

            public string FullName { get; set; }

            public string Result { get; set; }
        }
    }
}
