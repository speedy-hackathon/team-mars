using System;
using System.Collections.Generic;
using System.Linq;

namespace covidSim.Services
{
    public class Game
    {
        public List<Person> People;
        public CityMap Map;
        private DateTime _lastUpdate;

        private static Game _gameInstance;
        private static Random _random = new Random();
        
        public const int PeopleCount = 300;
        public const int FieldWidth = 1000;
        public const int FieldHeight = 500;
        public const int MaxPeopleInHouse = 10;

        private Game()
        {
            Map = new CityMap();
            People = CreatePopulation();
            _lastUpdate = DateTime.Now;
        }

        public static Game Instance => _gameInstance ?? (_gameInstance = new Game());

        private List<Person> CreatePopulation()
        {
            var healthyPeoples = CreatePeoples(
                (int) Math.Ceiling(PeopleCount * 0.95));
            var sickPeoples = CreatePeoples(
                (int) Math.Ceiling(PeopleCount * 0.05), true);

            return healthyPeoples.Concat(sickPeoples).ToList();
        }

        private IEnumerable<Person> CreatePeoples(int count, bool sick = false)
        {
            return Enumerable
                .Repeat(0, count)
                .Select((_, index) => new Person(index, FindHome(), Map, sick));
        }

        private int FindHome()
        {
            while (true)
            {
                var homeId = _random.Next(CityMap.HouseAmount);

                if (Map.Houses[homeId].ResidentCount < MaxPeopleInHouse)
                {
                    Map.Houses[homeId].ResidentCount++;
                    return homeId;
                }
            }
            
        }

        public Game GetNextState()
        {
            var diff = (DateTime.Now - _lastUpdate).TotalMilliseconds;
            if (diff >= 1000)
            {
                CalcNextStep();
            }

            return this;
        }

        private void CalcNextStep()
        {
            _lastUpdate = DateTime.Now;
            foreach (var person in People)
            {
                person.CalcNextStep();
            }
        }
    }
}