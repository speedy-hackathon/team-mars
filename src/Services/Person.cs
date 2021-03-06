using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using covidSim.Models;

namespace covidSim.Services
{
    public class Person
    {
        private const int MaxDistancePerTurn = 30;
        private const int StepsForHealing = 45;
        private const int MaxStartAge = 70;
        private static Random random = new Random();
        private PersonState state = PersonState.AtHome;
        private int age;
        private int stepHomeCount;
        private CityMap cityMap;

        public Person(int id, int homeId, CityMap map, InternalPersonState internalState = InternalPersonState.Healthy,
            bool isNew = false)

        {
            Id = id;
            age = isNew ? 0 : random.Next(MaxStartAge);
            HomeId = homeId;
            InternalState = internalState;
            cityMap = map;
            homeCoords = map.Houses[homeId].Coordinates.LeftTopCorner;
            Position = GetNewPersonAtHomePosition();
            nextPosition = GetNewPersonAtHomePosition();
        }

        public InternalPersonState InternalState;
        public int Id;
        public int HomeId;
        public Vec Position;
        private Vec nextPosition;
        private Vec homeCoords;
        private int stepsInDead;
        private int stepsWithSick;

        public void CalcNextStep(IEnumerable<Person> persons)
        {
            ProcessDeadState();
            
            if (InternalState == InternalPersonState.Dead)
                return;
           
            ProcessSickState();
            switch (state)
            {
                case PersonState.AtHome:
                    CalcNextStepForPersonAtHome();
                    break;
                case PersonState.Walking:
                    CalcNextPositionForWalkingPerson();
                    if (InternalState != InternalPersonState.Sick) 
                        CheckPersonForInfection(persons);
                    break;
                case PersonState.GoingHome:
                    CalcNextPositionForGoingHomePerson();
                    break;
            }
        }

        private void ProcessDeadState()
        {
            if (InternalState == InternalPersonState.Sick && random.NextDouble() < 0.00003)
            {
                InternalState = InternalPersonState.Dead;
                return;
            }

            if (InternalState == InternalPersonState.Dead)
                stepsInDead++;

            if (stepsInDead >= 10)
                InternalState = InternalPersonState.NeedDeleted;
        }

        private void CheckPersonForInfection(IEnumerable<Person> persons)
        {
            if (persons.Any(anotherPerson => DistanceBetweenPoints(Position, anotherPerson.Position) <= 7 &&
                                             anotherPerson.InternalState == InternalPersonState.Sick) &&
                random.NextDouble() <= 0.5)
                InternalState = InternalPersonState.Sick;
        }

        private int DistanceBetweenPoints(Vec first, Vec second)
        {
            return (int) Math.Sqrt((first.X - second.X) * (first.X - second.X) +
                                   (first.Y - second.Y) * (first.Y - second.Y));
        }

        private void ProcessSickState()
        {
            if (InternalState != InternalPersonState.Sick)
            {
                stepsWithSick = 0;
                return;
            }

            stepsWithSick++;
            if (stepsWithSick >= StepsForHealing)
                InternalState = InternalPersonState.Healthy;
        }

        public void IncreaseAge() => age++;

        private void CalcNextStepForPersonAtHome()
        {
            var goingWalk = random.NextDouble() < 0.005;
            if (!goingWalk)
            {
                if (nextPosition.X == Position.X && nextPosition.Y == Position.Y)
                {
                    nextPosition = GetNewPersonAtHomePosition();
                }

                stepHomeCount++;
                if (stepHomeCount >= 5 && InternalState != InternalPersonState.Bored)
                    InternalState = InternalPersonState.Bored;

                var distanceX = Math.Abs(Position.X - nextPosition.X);
                var distanceY = Math.Abs(Position.Y - nextPosition.Y);
                var deltaX = random.Next(MaxDistancePerTurn);
                var deltaY = random.Next(MaxDistancePerTurn);
                
                deltaX = Math.Min(distanceX, deltaX) * Math.Sign(nextPosition.X - Position.X);
                deltaY = Math.Min(distanceY, deltaY) * Math.Sign(nextPosition.Y - Position.Y);

                Position = new Vec(Position.X + deltaX, Position.Y + deltaY);

                return;
            }

            stepHomeCount = 0;
            InternalState = InternalPersonState.Healthy;
            state = PersonState.Walking;
            CalcNextPositionForWalkingPerson();
        }

        private Vec GetNewPersonAtHomePosition()
        {
            var x = homeCoords.X + random.Next(HouseCoordinates.Width);
            var y = homeCoords.Y + random.Next(HouseCoordinates.Height);
            return new Vec(x, y);
        }

        private void CalcNextPositionForWalkingPerson()
        {
            var xLength = random.Next(MaxDistancePerTurn);
            var yLength = MaxDistancePerTurn - xLength;
            var direction = ChooseDirection();
            var delta = new Vec(xLength * direction.X, yLength * direction.Y);
            var nextPosition = new Vec(Position.X + delta.X, Position.Y + delta.Y);

            if (isCoordInField(nextPosition) )
            {
                Position = nextPosition;
            }
            else
            {
                CalcNextPositionForWalkingPerson();
            }
        }
        
        private void CalcNextPositionForGoingHomePerson()
        {
            var game = Game.Instance;
            var homeCoord = game.Map.Houses[HomeId].Coordinates.LeftTopCorner;
            var homeCenter = new Vec(homeCoord.X + HouseCoordinates.Width / 2,
                homeCoord.Y + HouseCoordinates.Height / 2);

            var xDiff = homeCenter.X - Position.X;
            var yDiff = homeCenter.Y - Position.Y;
            var xDistance = Math.Abs(xDiff);
            var yDistance = Math.Abs(yDiff);

            var distance = xDistance + yDistance;
            if (distance <= MaxDistancePerTurn)
            {
                Position = homeCenter;
                state = PersonState.AtHome;
                return;
            }

            var direction = new Vec(Math.Sign(xDiff), Math.Sign(yDiff));

            var xLength = Math.Min(xDistance, MaxDistancePerTurn);
            var newX = Position.X + xLength * direction.X;
            var yLength = MaxDistancePerTurn - xLength;
            var newY = Position.Y + yLength * direction.Y;
            Position = new Vec(newX, newY);
        }

        public void GoHome()
        {
            if (state != PersonState.Walking) return;

            state = PersonState.GoingHome;
            CalcNextPositionForGoingHomePerson();
        }

        private Vec ChooseDirection()
        {
            var directions = new Vec[]
            {
                new Vec(-1, -1),
                new Vec(-1, 1),
                new Vec(1, -1),
                new Vec(1, 1),
            };
            var index = random.Next(directions.Length);
            return directions[index];
        }

        private bool isCoordInField(Vec vec)
        {
            var belowZero = vec.X < 0 || vec.Y < 0;
            var beyondField = vec.X > Game.FieldWidth || vec.Y > Game.FieldHeight;

            return !(belowZero || beyondField);
        }
    }
}