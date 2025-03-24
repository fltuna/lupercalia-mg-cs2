namespace LupercaliaMGCore {
    public class MapConfigFile {

        public string name {get;}
        public string path {get;}

        public MapConfigFile(string configName, string configPath) {
            name = configName;
            path = configPath;
        }
    }
}