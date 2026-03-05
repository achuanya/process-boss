using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SKM.Models;

namespace SKM.Services
{
    public class ConfigService
    {
        private const string ConfigFileName = "rules.json";
        private readonly string _configPath;

        public List<ProcessRule> Rules { get; private set; } = new List<ProcessRule>();

        public ConfigService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "SKM");
            Directory.CreateDirectory(appFolder);
            _configPath = Path.Combine(appFolder, ConfigFileName);
            LoadRules();
        }

        public void LoadRules()
        {
            if (File.Exists(_configPath))
            {
                try
                {
                    string json = File.ReadAllText(_configPath);
                    Rules = JsonSerializer.Deserialize<List<ProcessRule>>(json) ?? new List<ProcessRule>();
                }
                catch (Exception)
                {
                    // Handle error or log
                    Rules = new List<ProcessRule>();
                }
            }
        }

        public void SaveRules()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(Rules, options);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception)
            {
                // Handle error
            }
        }

        public void AddRule(ProcessRule rule)
        {
            Rules.Add(rule);
            SaveRules();
        }

        public void RemoveRule(ProcessRule rule)
        {
            Rules.Remove(rule);
            SaveRules();
        }
        
        public void UpdateRule(ProcessRule oldRule, ProcessRule newRule)
        {
            int index = Rules.IndexOf(oldRule);
            if (index != -1)
            {
                Rules[index] = newRule;
                SaveRules();
            }
        }
    }
}
