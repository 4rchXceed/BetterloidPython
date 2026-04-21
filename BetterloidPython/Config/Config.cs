using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BetterloidPython.Config
{
    public class Config
    {
        [JsonPropertyName("scripts")]
        public Dictionary<string, string> Scripts { get; set; } = new Dictionary<string, string>();
    }
}
