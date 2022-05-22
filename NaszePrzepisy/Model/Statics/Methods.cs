using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;

namespace NaszePrzepisy.Model.Statics
{
    
    public static class Methods
    {
        static string connectionString = "DefaultEndpointsProtocol=https;AccountName=tommo666;AccountKey=ytSf+HvlE3IVf/ctjUvyxSRebmumxSFwPNwxfDX0cWqF9fXhhjpj2nuz2N5Fum+vNMP8AevkU1rafjuZKW+hRg==;EndpointSuffix=core.windows.net";
        public static Dictionary<string, List<Przepis>> slownikPrzepisow;
        private static string xmlFileString = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NaszePrzepisy", "przepisy.xml");
        private static string xmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NaszePrzepisy");
        public static string[] poryPosilkow = { "Śniadania", "Obiady", "Kolacje" };
        public static int lastPrzepisId;

        internal static Dictionary<string, List<Przepis>> GetRecipes()
        {
            lastPrzepisId = 0;
            List<Przepis> listaPrzepisow = new List<Przepis>();
            Dictionary<string, List<Przepis>> result = new Dictionary<string, List<Przepis>>();
            XElement doc = XElement.Load(xmlFileString);
            foreach (string poraPosilku in poryPosilkow)
            {
                result.Add(poraPosilku, new List<Przepis>());
                var wybranyPosilek = doc.Elements(poraPosilku);
                var posilki = wybranyPosilek.Elements("Przepis");
                foreach (var posilek in posilki)
                {
                    Przepis przepis = new Przepis();
                    przepis.Posilek = poraPosilku;
                    przepis.Id = int.Parse(posilek.Attribute("Id").Value);
                    if (lastPrzepisId < przepis.Id)
                        lastPrzepisId = przepis.Id;
                    przepis.Nazwa = posilek.Elements("Nazwa").First().Value;
                    przepis.Skladniki = GetSkladniki(posilek.Elements("Skladniki"));
                    przepis.Przygotowanie = posilek.Elements("Przygotowanie").First().Value;
                    listaPrzepisow.Add(przepis);
                    result[poraPosilku].Add(przepis);
                }
            }
            return result;
        }

        internal static void CheckIfXMLExists()
        {
            if (!Directory.Exists(xmlPath))
                Directory.CreateDirectory(xmlPath);
            if (!File.Exists(xmlFileString))
            {

                using (XmlWriter writer = XmlWriter.Create(Path.Combine(xmlPath, "przepisy.xml")))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Posilki");
                    writer.WriteStartElement("Śniadania");
                    writer.WriteEndElement();
                    writer.WriteStartElement("Obiady");
                    writer.WriteEndElement();
                    writer.WriteStartElement("Kolacje");
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Close();
                }
            }
        }

        private static List<string> GetSkladniki(IEnumerable<XElement> skladniki)
        {
            List<string> result = new List<string>();
            foreach (var skladnik in skladniki.Elements())
            {
                result.Add(skladnik.Value);
            }
            return result;
        }

        internal static ObservableCollection<string> FillRecipes(string pora)
        {
            ObservableCollection<string> result = new ObservableCollection<string>();
            foreach (var posilek in slownikPrzepisow[pora])
            {
                result.Add(posilek.Nazwa);
            }
            return result;
        }

        internal static async void UploadRecipes()
        {
            DialogResult dialogResult = MessageBox.Show("Czy napewno chcesz wysłać przepisy na serwer?", "Na 100%??", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.No)
                return;
            Cursor.Current = Cursors.WaitCursor;
            BlobServiceClient blobserviceClient = new BlobServiceClient(connectionString);
            string containerName = "naszeprzepisy";
            string filename = "przepisy.xml";
            BlobContainerClient containerClient = blobserviceClient.GetBlobContainerClient(containerName);
            BlobClient blobClient = containerClient.GetBlobClient(filename);

            // backup starego na serwerze
            string backupPath = xmlFileString.Replace(".xml", DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xml");
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            using (FileStream downloadFileStream = File.OpenWrite(backupPath))
            {
                await download.Content.CopyToAsync(downloadFileStream);
                downloadFileStream.Close();
            }
            BlobClient blobClientBackup = containerClient.GetBlobClient(Path.GetFileName(backupPath));
            FileStream uploadFileStreamBackup = File.OpenRead(backupPath);
            await blobClientBackup.UploadAsync(uploadFileStreamBackup, true);
            uploadFileStreamBackup.Close();
            //

            FileStream uploadFileStream = File.OpenRead(xmlFileString);
            await blobClient.UploadAsync(uploadFileStream, true);
            uploadFileStream.Close();
            File.Delete(backupPath);
            Cursor.Current = Cursors.Default;
            MessageBox.Show("Wysłano na serwer", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        internal static int GetPrzepisID()
        {
            List<int> currentID = new List<int>();
            foreach (var pora in slownikPrzepisow)
            {
                pora.Value.ForEach(x => currentID.Add(x.Id));
            }
            int maxID = currentID.Max();
            return maxID + 1;
        }

        public static ObservableCollection<string> GetIngridients(int seletedIndex, string wybranyPosilek)
        {
            ObservableCollection<string> result = new ObservableCollection<string>();
            if (seletedIndex >= 0 && !string.IsNullOrEmpty(wybranyPosilek))
            {
                var przepis = slownikPrzepisow[wybranyPosilek][seletedIndex];
                foreach (var skladnik in przepis.Skladniki)
                {
                    result.Add(skladnik);
                }
            }
            return result;
        }


        internal static string GetInstruction(int seletedIndex,string wybranyPosilek)
        {
            if (seletedIndex >= 0 && !string.IsNullOrEmpty(wybranyPosilek))
                return slownikPrzepisow[wybranyPosilek][seletedIndex].Przygotowanie;
            else
                return string.Empty;
        }

        internal static ObservableCollection<string> ConvertArrayToObsCollection()
        {
            ObservableCollection<string> result = new ObservableCollection<string>();
            foreach (string pora in poryPosilkow)
                result.Add(pora);
            return result;

        }

        internal static List<string> ConvertToList(ObservableCollection<string> ingridients)
        {
            List<string> result = new List<string>();
            foreach (string item in ingridients)
            {
                result.Add(item);
            }
            return result;
        }

        internal static void UpdateXML(Przepis przepis, bool czyNowyPrzepis)
        {
            XDocument doc = XDocument.Load(xmlFileString);
            var tempDoc = new XElement("Przepis",
                            new XAttribute("Id",przepis.Id),
                            new XElement("Nazwa", przepis.Nazwa),
                            new XElement("Skladniki",
                                GetSkladnikiFromAdder(przepis.Skladniki)),
                            new XElement("Przygotowanie", przepis.Przygotowanie));
            if (!czyNowyPrzepis)
                doc.Descendants("Przepis").Where(x => x.Attribute("Id").Value == przepis.Id.ToString()).Remove();
            doc.Element("Posilki").Element(przepis.Posilek).Add(tempDoc);
            doc.Save(xmlFileString);
            slownikPrzepisow = GetRecipes();
            Messenger.Default.Send(przepis.Posilek, "dodanoprzepis");
        }

        private static List<XElement> GetSkladnikiFromAdder(List<string> skladniki)
        {
            List<XElement> result = new List<XElement>();
            foreach (var item in skladniki)
                result.Add(new XElement("Skladnik", item));
            return result;
        }


        internal static void RemoveSelectedRecipe(int selectedId,string wybranyprzepis)
        {
            if (selectedId == 666666)
            {
                MessageBox.Show("Ale nie wybrano przepisu :(", "Ojej", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult dialogResult = MessageBox.Show("Czy napewno chcesz usunąć wybrany przepis?", "Na 100%??", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.Yes)
            {
                XDocument doc = XDocument.Load(xmlFileString);
                doc.Descendants("Przepis").Where(x => x.Attribute("Id").Value == selectedId.ToString()).Remove();
                doc.Save(xmlFileString);
                slownikPrzepisow = GetRecipes();
                Messenger.Default.Send(wybranyprzepis, "dodanoprzepis");
            }            
        }
    }
}
