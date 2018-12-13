// Copyright (c) Bespinglobal Corporation. All rights reserved.

using Newtonsoft.Json;

using System;

using System.Collections.Generic;

using System.Net;
namespace PreProcessorModule
{

    public enum Environment
    {
        productionOnlinux,
        testOnWindow, pri
    }
    public class MessageBody
    {
        [JsonProperty("line")]
        public string LineName { get; set; }
        [JsonProperty("raw")]
        public string Raw { get; set; }
        [JsonProperty("cep")]
        public string Cep { get; set; }//True False Unkown

        [JsonProperty("predicted")]
        public string Predicted { get; set; }//True False Unkown
    }
    public class ModuleMessageBody
    {
        [JsonProperty("line")]
        public string LineName { get; set; }

        [JsonProperty("badproductinfo")]
        public BadProductInfo BadProductInfo { get; set; }

        [JsonProperty("raw")]
        public string Raw { get; set; }
        [JsonProperty("cep")]
        public string Cep; //True False Unkown
        [JsonProperty("Aps")]
        public string Aps { get; set; }
    }
    public class DateFolderInfo //: IEquatable<Part>
    {
        public int VerifiyingID;
        public DateTime WorkingDate { get; set; }
        public int ToalBadProductsNumbers { get; set; }
        // public Queue<BadProductInfo> BadProductsWithErrors { get; set; }
        public Queue<BadProductInfo> BadProductsToPass { get; set; }
        public string DateFolderLocationUnderReport { get; set; }
        public string APSFolderLocation { get; set; }
        public string CepFolderLocation { get; set; }
        public string RawDataFolderLocation { get; set; }
        public bool isProcessingComplete { get; set; }

        public override string ToString()
        {
            return "ID: " + VerifiyingID + "   Date: " + WorkingDate;
        }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            DateFolderInfo objAsPart = obj as DateFolderInfo;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        public override int GetHashCode()
        {
            return VerifiyingID;
        }
        public bool Equals(DateFolderInfo other)
        {
            if (other == null) return false;
            return (this.WorkingDate.Equals(other.WorkingDate));
        }
        // Should also override == and != operators.
    }

    // Bad product is without empty string in barcode. 
    public class BadProductInfo //: IEquatable<Part>
    {
        public string Date { get; set; }
        public string Model { get; set; }
        public string BarCode { get; set; }
        public string Result { get; set; }

        public override string ToString()
        {
            return "Model: " + Model + "   BarCode: " + BarCode;
        }

    }
}
