using System;
using covidSim.Models;

namespace covidSim.Services
{
    public class Person
    {
        private const int MaxDistancePerTurn = 30;
        private static Random random = new Random();
        private PersonState state = PersonState.AtHome;
        private int stepHomeCount;
        private CityMap cityMap;

        public Person(int id, int homeId, CityMap map, InternalPersonState internalState = InternalPersonState.None)
        {
            Id = id;
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

        public void CalcNextStep()
        {
            switch (state)
            {
                case PersonState.AtHome:
                    CalcNextStepForPersonAtHome();
                    break;
                case PersonState.Walking:
                    CalcNextPositionForWalkingPerson();
                    break;
                case PersonState.GoingHome:
                    CalcNextPositionForGoingHomePerson();
                    break;
            }
        }

        private void CalcNextStepForPersonAtHome()
        {
            var goingWalk = random.NextDouble() < 0.005;
            if (!goingWalk)
            {
                if (nextPosition.X == Position.X && nextPosition.Y == Position.Y)
                {
                    nextPosition = GetNewPersonAtHomePosition();
                }

                var distanceX = Math.Abs(Position.X - nextPosition.X);
                var distanceY = Math.Abs(Position.Y - nextPosition.Y);
                var deltaX = random.Next(MaxDistancePerTurn);
                var deltaY = random.Next(MaxDistancePerTurn);
                
                deltaX = Math.Min(distanceX, deltaX) * Math.Sign(nextPosition.X - Position.X);
                deltaY = Math.Min(distanceY, deltaY) * Math.Sign(nextPosition.Y - Position.Y);

                Position = new Vec(Position.X + deltaX, Position.Y + deltaY);

                return;
            }

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
                if (IsPersonInHome(nextPosition, cityMap.Houses[HomeId].Coordinates))
                {
                    stepHomeCount++;
                    if (stepHomeCount >= 5 && InternalState != InternalPersonState.Bored)
                        InternalState = InternalPersonState.Bored;
                }
                else
                {
                    stepHomeCount = 0;
                    InternalState = InternalPersonState.None;
                }
                
                Position = nextPosition;
            }
            else
            {
                CalcNextPositionForWalkingPerson();
            }
        }

        private bool IsPersonInHome(Vec nextPos, HouseCoordinates coordinates)
        {
            return (coordinates.LeftTopCorner.X <= nextPos.X) && (coordinates.LeftTopCorner.X + HouseCoordinates.Width >= nextPos.X) &&
                (coordinates.LeftTopCorner.Y <= nextPos.Y) && (coordinates.LeftTopCorner.Y + HouseCoordinates.Height >= nextPos.Y);
        }

        private void CalcNextPositionForGoingHomePerson()
        {
            var game = Game.Instance;
            var homeCoord = game.Map.Houses[HomeId].Coordinates.LeftTopCorner;
            var homeCenter = new Vec(homeCoord.X + HouseCoordinates.Width / 2, homeCoord.Y + HouseCoordinates.Height / 2);

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