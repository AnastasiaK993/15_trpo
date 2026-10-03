using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElectronicsStore.Models;

namespace ElectronicsStore.Services
{
    public class DBService
    {
        private readonly ElectronicsStoreContext _context;
        public ElectronicsStoreContext Context => _context;

        private static DBService? _instance;
        public static DBService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DBService();
                return _instance;
            }
        }

        private DBService()
        {
            _context = new ElectronicsStoreContext();
        }
    }
}