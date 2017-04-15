using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sentiment.QWordNet
{
    public class QWordNetDB
    {

        private static string QWORDNET_FILE_LOCATION = System.Configuration.ConfigurationSettings.AppSettings["qwordnet"] + @"\qwordnet\qwordnet-30-0.3.xml";

        public Dictionary<String, QWordNetPolarityCounter> polarities = new Dictionary<String, QWordNetPolarityCounter>();
        public Dictionary<String, String> posMappings = new Dictionary<String, String>();

        private QWordNetDB()
        {
            this.posMappings.Add("JJ", "a");
            this.posMappings.Add("JJR", "a");
            this.posMappings.Add("JJS", "a");
            this.posMappings.Add("NN", "n");
            this.posMappings.Add("NNS", "n");
            this.posMappings.Add("NP", "n");
            this.posMappings.Add("NPS", "n");
            this.posMappings.Add("RB", "r");
            this.posMappings.Add("RBR", "r");
            this.posMappings.Add("RBS", "r");
            this.posMappings.Add("VB", "v");
            this.posMappings.Add("VBD", "v");
            this.posMappings.Add("VBG", "v");
            this.posMappings.Add("VBN", "v");
            this.posMappings.Add("VBP", "v");
            this.posMappings.Add("VBZ", "v");
        }

        public static QWordNetDB createInstance()
        {
            QWordNetDB qWordNetDB = new QWordNetDB();
            qWordNetDB.load();
            return qWordNetDB;
        }


        private void load()
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(QWORDNET_FILE_LOCATION);
            XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("sense");

            foreach (XmlNode node in nodeList)
            {
                XmlAttributeCollection attCol = node.Attributes;
                var lemma = node.SelectNodes("lemma");
                QWordNetXMLSAXParserHandler paser = new QWordNetXMLSAXParserHandler();
                paser.startElement("sense", attCol);
                foreach (XmlNode lem in lemma)
                {
                    paser.endElement("lemma", lem.InnerText, this.polarities);
                }

            }



        }
        public int getPolarity(String lemma, String pos)
        {
            
            if (this.posMappings.ContainsKey(pos))
            {
                String index = lemma + "." + this.posMappings[pos];
                if (this.polarities.ContainsKey(index))
                {
                    var polarity = this.polarities[index];
                    return polarity.getPolarity();
                }
                else return 0;
            }
            else return 0;
        }
    }




    public class QWordNetPolarityCounter
    {
        private int posCount = 0;
        private int negCount = 0;

        public void incrementPositive()
        {
            this.posCount++;
        }

        public void incrementNegative()
        {
            this.negCount++;
        }

        public int getPolarity()
        {
            if (this.posCount < this.negCount)
            {
                return -1;
            }
            else return 1;

        }
    }



    public class QWordNetXMLSAXParserHandler
    {

        private static String SENSE_TAG = "sense";
        private static String SENSE_POLARITY_ATTRIBUTE = "polarity";
        private static String SENSE_POSITIVE_POLARITY_VALUE = "1";
        private static String SENSE_PART_OF_SPEECH_ATTRIBUTE = "pos";

        private static String LEMMA_TAG = "lemma";

        public StringBuilder text = new StringBuilder();
        public String polarity;
        public String pos;


        public void startElement(String qName, XmlAttributeCollection attributes)
        {
            if (qName.Equals(SENSE_TAG))
            {
                this.polarity = attributes[SENSE_POLARITY_ATTRIBUTE].Value;
                this.pos = attributes[SENSE_PART_OF_SPEECH_ATTRIBUTE].Value;
                if (this.pos.Equals("s"))
                {
                    this.pos = "a";
                }
            }
            else if (qName.Equals(LEMMA_TAG))
            {
                this.text = new StringBuilder();
            }
        }


        public void endElement(String qName, String lvalue, Dictionary<String, QWordNetPolarityCounter> polarities)
        {
            if (qName.Equals(LEMMA_TAG))
            {
                String index = lvalue + "." + this.pos;
                if (!polarities.ContainsKey(index))
                {
                    polarities.Add(index, new QWordNetPolarityCounter());
                }

                if (this.polarity.Equals(SENSE_POSITIVE_POLARITY_VALUE))
                {
                    polarities[index].incrementPositive();
                }
                else
                {
                    polarities[index].incrementNegative();
                }
            }
        }


    }

}
