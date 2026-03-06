using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ProcessBoss.Models;

namespace ProcessBoss.Services
{
    public class ConfigService
    {
        private const string ConfigFileName = "rules.json";
        private readonly string _configPath;

        public event Action<ProcessRule>? RuleAdded;
        public event Action<ProcessRule>? RuleRemoved;
        public event Action<ProcessRule>? RuleUpdated;

        public List<ProcessRule> Rules { get; private set; } = new List<ProcessRule>();

        public ConfigService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appFolder = Path.Combine(appDataPath, "ProcessBoss");
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
            RuleAdded?.Invoke(rule);
        }

        public void RemoveRule(ProcessRule rule)
        {
            Rules.Remove(rule);
            SaveRules();
            RuleRemoved?.Invoke(rule);
        }
        
        public void UpdateRule(ProcessRule oldRule, ProcessRule newRule)
        {
            int index = Rules.IndexOf(oldRule);
            if (index != -1)
            {
                Rules[index] = newRule;
                SaveRules();
                RuleUpdated?.Invoke(newRule);
            }
        }
    }
}
