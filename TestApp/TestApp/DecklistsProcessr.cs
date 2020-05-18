using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TestApp
{
    class DecklistsProcessr
    {
        public static Deck generateDeckCode(string jsonList)
        {
            Deck d = JsonConvert.DeserializeObject<Deck>(jsonList);
            return d;
        }
    }
}
