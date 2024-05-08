using System.Collections.Generic;

namespace XivVoices.Engine
{
    public class DataMapper
    {
        private Dictionary<int, string> bodyMap = new Dictionary<int, string>()
        {
            {0, "Beastman"},
            {1, "Adult"},
            {3, "Elderly"},
            {4, "Child"},
        };

        private Dictionary<int, string> raceMap = new Dictionary<int, string>()
        {
            {1, "Hyur"},
            {2, "Elezen"},
            {3, "Lalafell"},
            {4, "Miqo'te"},
            {5, "Roegadyn"},
            {6, "Au Ra"},
            {7, "Hrothgar"},
            {8, "Viera"},
        };

        private Dictionary<int, string> tribeMap = new Dictionary<int, string>()
        {
            {1, "Midlander"},
            {2, "Highlander"},
            {3, "Wildwood"},
            {4, "Duskwight"},
            {5, "Plainsfolk"},
            {6, "Dunesfolk"},
            {7, "Seeker of the Sun"},
            {8, "Keeper of the Moon"},
            {9, "Sea Wolf"},
            {10, "Hellsguard"},
            {11, "Raen"},
            {12, "Xaela"},
            {13, "Helions"},
            {14, "The Lost"},
            {15, "Rava"},
            {16, "Veena"},
        };

        private Dictionary<int, string> eyesMap = new Dictionary<int, string>()
        {
            {0, "Option 1"},
            {1, "Option 2"},
            {2, "Option 3"},
            {3, "Option 4"},
            {4, "Option 5"},
            {5, "Option 6"},
            {128, "Option 1"},
            {129, "Option 2"},
            {130, "Option 3"},
            {131, "Option 4"},
            {132, "Option 5"},
            {133, "Option 6"},
        };

        private Dictionary<(int, ushort), string> skeletonRegionMap = new Dictionary<(int, ushort), string>()
        {
            {(60,0), "Dragon_Medium"}, // Medium size --> Sooh Non
            {(60,398), "Dragon_Medium"},
            {(63,0), "Dragon_Large"}, // Large size --> Ess Khas
            {(63,398), "Dragon_Large"},
            {(239,0), "Dragon_Small"}, // Small size --> Khash Thah
            {(239,398), "Dragon_Small"},

            {(405, 0), "Namazu"},
            {(494, 0), "Namazu"},
            {(494, 614), "Namazu"},
            {(494, 622), "Namazu"},

            {(706,0), "Ea"},
            {(706,960), "Ea"},

            {(11001, 0), "Amalj'aa"},
            {(11001, 146), "Amalj'aa"},
            {(11001, 401), "Vanu Vanu"},

            {(11002,0), "Ixal"},
            {(11002,154), "Ixal"},

            {(11003,0), "Kobold"},
            {(11003,180), "Kobold"},

            {(11004,0), "Goblin"},
            {(11004,478), "Goblin"},

            {(11005,0), "Sylph"},
            {(11005,152), "Sylph"},

            {(11006,0), "Moogle"},
            {(11006,400), "Moogle"},

            {(11007,0), "Sahagin"},
            {(11007,138), "Sahagin"},

            {(11008,0), "Mamool Ja"},
            {(11008,129), "Mamool Ja"},

            {(11009,0), "Matanga"},     // TODO: Find Areas for Them
            {(11009,1), "Giant"},       // TODO: Make a Giant Voice

            {(11012,0), "Qiqirn"},
            {(11013,0), "Qiqirn"},
            {(11013,139), "Qiqirn"},

            {(11016,0), "Skeleton"},    // TODO: Make a Dead Voice

            {(11020,0), "Vath"},
            {(11020,398), "Vath"},

            {(11028,0), "Kojin"},
            {(11028,613), "Kojin"},

            {(11029,0), "Ananta"},

            {(11030,0), "Lupin"},

            {(11038,0), "Pixie"},
            {(11038,816), "Pixie"},

            {(11051,0), "Omicron"},
            {(11051,960), "Omicron"},

            {(11052,0), "Loporrit"},
            {(11052,959), "Loporrit"}
        };

        public string GetBody(int id) => bodyMap.TryGetValue(id, out var name) ? name : "Adult";
        public string GetRace(int id) => raceMap.TryGetValue(id, out var name) ? name : "Unknown:" + id.ToString();
        public string GetTribe(int id) => tribeMap.TryGetValue(id, out var name) ? name : "Unknown:" + id.ToString();
        public string GetEyes(int id) => eyesMap.TryGetValue(id, out var name) ? name : "Unknown:" + id.ToString();
        public string GetSkeleton(int id, ushort region)
        {
            if (skeletonRegionMap.TryGetValue((id, region), out var name))
            {
                return name;
            }
            else if (skeletonRegionMap.TryGetValue((id, 0), out var defaultName))
            {
                return defaultName;
            }
            return "Unknown combination: ID " + id.ToString() + ", Region " + region.ToString();
        }
    }
}
