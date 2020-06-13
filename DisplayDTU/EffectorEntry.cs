using STRINGS;

namespace DisplayDTU
{
    class EffectorEntry
    {
        public string name;
        public int count;
        public float value;

        public EffectorEntry(string name, float value = 0)
        {
            this.name = name;
            this.value = value;
            this.count = 0;
        }
    }
}
