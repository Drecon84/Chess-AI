using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class XMLSaving 
{
    public XMLSaving(){

    }

    public void Save(string filePath, PlayerList players)
    {
        XmlSerializer writer = new XmlSerializer(typeof(PlayerList));
        FileStream stream = new FileStream(filePath, FileMode.Create);
        writer.Serialize(stream, players);
        stream.Close();
    }

    public PlayerList Load(string filePath)
    {
        try{
            FileStream stream = new FileStream(filePath, FileMode.Open);
            XmlSerializer reader = new XmlSerializer(typeof(PlayerList));
            PlayerList list = reader.Deserialize(stream) as PlayerList;
            stream.Close();

            return list;
        }
        catch (FileNotFoundException ex){
            return null;
        }
        
    }


   
}
