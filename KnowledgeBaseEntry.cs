﻿using Azure.Search.Documents.Indexes;

namespace Botina
{
    public class KnowledgeBaseEntry
    {
        [SimpleField(IsKey = true)]
        public string id { get; set; }
        [SearchableField]
        public string Department { get; set; }
        [SearchableField]
        public string Topic { get; set; }
        [SearchableField]
        public string Body { get; set; }
        [SearchableField]
        public string Owner { get; set; }
    }
}
