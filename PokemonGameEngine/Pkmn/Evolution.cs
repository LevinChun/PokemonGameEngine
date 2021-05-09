﻿using Kermalis.PokemonBattleEngine.Data;
using Kermalis.PokemonGameEngine.Item;
using Kermalis.PokemonGameEngine.Pkmn.Pokedata;
using Kermalis.PokemonGameEngine.World;
using System;
using System.Collections.Generic;

namespace Kermalis.PokemonGameEngine.Pkmn
{
    internal static class Evolution
    {
        private static readonly Queue<(PartyPokemon, EvolutionData.EvoData)> _pendingEvolutions = new Queue<(PartyPokemon, EvolutionData.EvoData)>(PkmnConstants.PartyCapacity);

        public static void AddPendingEvolution(PartyPokemon pkmn, EvolutionData.EvoData evo)
        {
            _pendingEvolutions.Enqueue((pkmn, evo));
        }
        public static (PartyPokemon, EvolutionData.EvoData)? GetNextPendingEvolution()
        {
            if (_pendingEvolutions.Count == 0)
            {
                return null;
            }
            return _pendingEvolutions.Dequeue();
        }

        private static bool IsNight(DateTime dt)
        {
            Month month = OverworldTime.GetMonth((Month)dt.Month);
            Season season = OverworldTime.GetSeason(month);
            int hour = OverworldTime.GetHour(dt.Hour);
            return OverworldTime.GetTimeOfDay(season, hour) == TimeOfDay.Night;
        }

        // Ignores Shedinja_LevelUp & Beauty_LevelUp
        // TODO: NosepassMagneton_Location_LevelUp, Leafeon_Location_LevelUp, Glaceon_Location_LevelUp
        public static EvolutionData.EvoData GetLevelUpEvolution(Party party, PartyPokemon pkmn)
        {
            bool isNight = IsNight(DateTime.Now);

            var data = new EvolutionData(pkmn.Species, pkmn.Form);
            foreach (EvolutionData.EvoData evo in data.Evolutions)
            {
                bool isMatch;
                switch (evo.Method)
                {
                    case EvoMethod.Friendship_LevelUp: isMatch = pkmn.Friendship >= evo.Param; break;
                    case EvoMethod.Friendship_Day_LevelUp: isMatch = !isNight && pkmn.Friendship >= evo.Param; break;
                    case EvoMethod.Friendship_Night_LevelUp: isMatch = isNight && pkmn.Friendship >= evo.Param; break;
                    case EvoMethod.LevelUp:
                    case EvoMethod.Ninjask_LevelUp: isMatch = pkmn.Level >= evo.Param; break;
                    case EvoMethod.ATK_GT_DEF_LevelUp: isMatch = pkmn.Level >= evo.Param && pkmn.Attack > pkmn.Defense; break;
                    case EvoMethod.ATK_EE_DEF_LevelUp: isMatch = pkmn.Level >= evo.Param && pkmn.Attack == pkmn.Defense; break;
                    case EvoMethod.ATK_LT_DEF_LevelUp: isMatch = pkmn.Level >= evo.Param && pkmn.Attack < pkmn.Defense; break;
                    case EvoMethod.Silcoon_LevelUp: isMatch = pkmn.Level >= evo.Param && ((pkmn.PID >> 0x10) % 10) <= 4; break;
                    case EvoMethod.Cascoon_LevelUp: isMatch = pkmn.Level >= evo.Param && ((pkmn.PID >> 0x10) % 10) > 4; break;
                    case EvoMethod.Item_Day_LevelUp: isMatch = !isNight && pkmn.Item == (ItemType)evo.Param; break;
                    case EvoMethod.Item_Night_LevelUp: isMatch = isNight && pkmn.Item == (ItemType)evo.Param; break;
                    case EvoMethod.Move_LevelUp: isMatch = pkmn.Moveset.Contains((PBEMove)evo.Param); break;
                    case EvoMethod.Male_LevelUp: isMatch = pkmn.Level >= evo.Param && pkmn.Gender == PBEGender.Male; break;
                    case EvoMethod.Female_LevelUp: isMatch = pkmn.Level >= evo.Param && pkmn.Gender == PBEGender.Female; break;
                    case EvoMethod.PartySpecies_LevelUp:
                    {
                        isMatch = false;
                        foreach (PartyPokemon p in party)
                        {
                            if (p != pkmn && !p.IsEgg && p.Species == (PBESpecies)evo.Param)
                            {
                                isMatch = true;
                                break;
                            }
                        }
                        break;
                    }
                    default: isMatch = false; break;
                }
                if (isMatch)
                {
                    return evo;
                }
            }
            return null;
        }

        public static EvolutionData.EvoData GetItemEvolution(PartyPokemon pkmn, ItemType item)
        {
            bool isNight = IsNight(DateTime.Now);

            var data = new EvolutionData(pkmn.Species, pkmn.Form);
            foreach (EvolutionData.EvoData evo in data.Evolutions)
            {
                if (item != (ItemType)evo.Param)
                {
                    continue;
                }
                bool isMatch;
                switch (evo.Method)
                {
                    case EvoMethod.Stone: isMatch = true; break;
                    case EvoMethod.Male_Stone: isMatch = pkmn.Gender == PBEGender.Male; break;
                    case EvoMethod.Female_Stone: isMatch = pkmn.Gender == PBEGender.Female; break;
                    case EvoMethod.Item_Day_LevelUp: isMatch = !isNight; break;
                    case EvoMethod.Item_Night_LevelUp: isMatch = isNight; break;
                    default: isMatch = false; break;
                }
                if (isMatch)
                {
                    return evo;
                }
            }
            return null;
        }

        public static EvolutionData.EvoData GetTradeEvolution(PartyPokemon pkmn, PBESpecies otherSpecies)
        {
            var data = new EvolutionData(pkmn.Species, pkmn.Form);
            foreach (EvolutionData.EvoData evo in data.Evolutions)
            {
                bool isMatch;
                switch (evo.Method)
                {
                    case EvoMethod.Trade: isMatch = true; break;
                    case EvoMethod.Item_Trade: isMatch = pkmn.Item == (ItemType)evo.Param; break;
                    case EvoMethod.ShelmetKarrablast:
                    {
                        isMatch = (pkmn.Species == PBESpecies.Shelmet && otherSpecies == PBESpecies.Karrablast)
                            || (pkmn.Species == PBESpecies.Karrablast && otherSpecies == PBESpecies.Shelmet);
                        break;
                    }
                    default: isMatch = false; break;
                }
                if (isMatch)
                {
                    return evo;
                }
            }
            return null;
        }
    }
}
