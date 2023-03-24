namespace Compiler
{
    public class SymTable
    {
        Dictionary<string, Symbol> _data;
        public Dictionary<string, Symbol> GetData()
        {
            return _data;
        }
        public int GetSize()
        {
            return _data.Count;
        }
        public void Add(string name, Symbol value)
        {
            if (_data.TryAdd(name, value))
            {
                return;
            }
            else
            {
                throw new Exception($"Duplicate identifier \"{name}\"");
            }
        }
        public Symbol Get(string name)
        {
            Symbol? value;
            if (_data.TryGetValue(name, out value))
            {
                return value;
            }
            else
            {
                throw new Exception("Variable not declared");
            }
        }
        public SymTable(Dictionary<string, Symbol> data)
        {
            _data = data;
        }
        public SymTable(SymTable original)
        {
            _data = new Dictionary<string, Symbol>(original._data);
        }
    }
    public class SymTableStack
    {
        List<SymTable> _tables;
        public int GetCountTables()
        {
            return _tables.Count;
        }
        public SymTable GetTable(int index)
        {
            return _tables[index];
        }
        public SymTable GetBackTable()
        {
            return _tables[^1];
        }
        public void AddTable(SymTable table)
        {
            _tables.Add(table);
        }
        public void PopBack()
        {
            _tables.RemoveAt(_tables.Count - 1);
        }
        public void Check(string name)
        {
            if (GetBackTable().GetData().ContainsKey(name))
            {
                throw new Exception($"Duplicate identifier \"{name}\"");
            }
        }
        public void Add(string name, Symbol value)
        {
            try
            {
                GetBackTable().Add(name, value);
            }
            catch
            {
                throw new Exception($"Duplicate identifier \"{name}\"");
            }
        }
        public Symbol Get(string name)
        {
            Symbol res = new Symbol("");
            bool found = false;
            for (int i = _tables.Count - 1; i >= 0; i--)
            {
                try
                {
                    res = _tables[i].Get(name);
                }
                catch
                {
                    continue;
                }
                finally
                {
                    found = true;
                }
            }
            if (!found)
            {
                throw new Exception("Variable not declared");
            }
            return res;
        }
        public SymTableStack()
        {
            _tables = new List<SymTable>();
        }
    }
}
