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
        
        public const int PeopleCount = 320;
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

        public static Game Restart() => new Game();

        private List<Person> CreatePopulation()
        {
            var sickPeopleCount = (int) Math.Ceiling(PeopleCount * 0.05);
            
            var healthyPeople = CreatePeoples(
                PeopleCount - sickPeopleCount);
            var sickPeople = CreatePeoples(
                sickPeopleCount, InternalPersonState.Sick);

            return healthyPeople.Concat(sickPeople).ToList();
        }

        private IEnumerable<Person> CreatePeoples(int count, InternalPersonState internalPersonState = InternalPersonState.None)
        {
            return Enumerable
                .Repeat(0, count)
                .Select((_, index) => new Person(index, FindHome(), Map, internalPersonState));
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
            foreach (var person in People.ToArray())
            {
                person.CalcNextStep(People);
                if (person.InternalState == InternalPersonState.NeedDeleted)
                    People.Remove(person);
            }
        }
    }
}