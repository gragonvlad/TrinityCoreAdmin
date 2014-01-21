﻿using System.Threading.Tasks;
using System.Data;
using System;

namespace TrinityCoreAdmin
{
    internal class Player
    {
        public Player()
        { }

        public Class _class
        { get; private set; }

        public uint acccountId
        { get; private set; }

        public Gender gender
        { get; private set; }

        public uint guid
        { get; private set; }

        public int money
        { get; private set; }

        public string name
        { get; set; }

        public Race race
        { get; private set; }

        public static async Task<Player> LoadPlayer(int guid)
        {
            Player p = new Player();
            await p.LoadFromDB(guid);
            return p;
        }

        private async Task LoadFromDB(int guid)
        {
            var stmt = ServerManager.charDB.GetPreparedStatement(CharDatabase.CharDatabaseStatements.CHAR_SEL_CHARACTER);
            stmt.Parameters.AddWithValue("@guid", guid);
            var dt = await ServerManager.charDB.Execute(stmt, false);

            if (dt.Rows.Count == 0)
            { 
                Logger.LOG_DATABASE.Error("Player with GUID " + guid.ToString() + " not found in table `characters`, can't load.");
                return;
            }
                
            if (XConverter.ToInt32(dt.Rows[0][0]) != guid)
            {
                Logger.LOG_DATABASE.Error("Error during loading player with GUID " + guid.ToString() + ".");
                return;
            }
                
            this.guid = XConverter.ToUInt32(dt.Rows[0][0]);
            this.acccountId = XConverter.ToUInt32(dt.Rows[0][1]);
            this.name = dt.Rows[0][2].ToString();
            this.race = (Race)Enum.ToObject(typeof(Race), XConverter.ToInt32(dt.Rows[0][3]));
            this._class = (Class)Enum.ToObject(typeof(Class), dt.Rows[0][4]);
            this.gender = (Gender)Enum.ToObject(typeof(Gender), dt.Rows[0][5]);
            this.money = XConverter.ToInt32(dt.Rows[0][8]);
        }
    }
}