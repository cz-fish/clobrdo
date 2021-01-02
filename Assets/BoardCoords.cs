using System;
using UnityEngine;

namespace Assets
{
    public static class BoardCoords
    {
        public static String[] playerOrder = {"blue", "yellow", "red", "green"};

        public static Vector3 getHomeCoords(int playerId, int pieceNumber)
        {
            float baseX, baseZ;
            var deltaX = 3.404f - 2.667f;
            var deltaZ = 3.65f - 2.865f;

            if (playerId == 1 || playerId == 2)
            {
                // left side
                baseX = -3.395f;
            }
            else
            {
                // right side
                baseX = 3.404f;
                deltaX = -deltaX;
            }

            if (playerId == 2 || playerId == 3)
            {
                // top side
                baseZ = 2.865f;
            }
            else
            {
                baseZ = -2.858f;
                deltaZ = -deltaZ;
            }

            int x = pieceNumber / 2;
            int z = pieceNumber % 2;
            return new Vector3(
                baseX + x * deltaX,
                0f,
                baseZ + z * deltaZ
            );
        }

        public static Vector3 getTargetCoords(int playerId, int pieceNumber)
        {
            const float min = 0.983f;
            const float max = 3.386f;
            var delta = (max - min) / 3;
            switch (playerId)
            {
                case 0:
                    // blue - horizontal right
                    return new Vector3(max - pieceNumber * delta, 0, 0);
                case 1:
                    // yellow - vertical bottom
                    return new Vector3(0, 0, -max + pieceNumber * delta);
                case 2:
                    // red - horizontal left
                    return new Vector3(-max + pieceNumber * delta, 0, 0);
                case 3:
                    // green - vertical top
                    return new Vector3(0, 0, max - pieceNumber * delta);
            }
            return new Vector3(0, 0, 0);
        }

        public static int getStartingPosition(int playerId)
        {
            return playerId * 10;
        }

        public static Vector3 getPositionCoords(int position)
        {
            const float max = 4.392f;
            var delta = 2 * max / 10;
            if (position <= 4)
            {
                return new Vector3(max - position * delta, 0, -delta);
            }
            else if (position <= 8)
            {
                var d = position - 4 + 1;
                return new Vector3(delta, 0, -d * delta);
            }
            else if (position == 9)
            {
                return new Vector3(0, 0, -max);
            }
            else if (position <= 14)
            {
                var d = position - 10;
                return new Vector3(-delta, 0, -max + d * delta);
            }
            else if (position <= 18)
            {
                var d = position - 14 + 1;
                return new Vector3(-d * delta, 0, -delta);
            }
            else if (position == 19)
            {
                return new Vector3(-max, 0, 0);
            }
            else if (position <= 24)
            {
                var d = position - 20;
                return new Vector3(-max + d * delta, 0, delta);
            }
            else if (position <= 28)
            {
                var d = position - 24 + 1;
                return new Vector3(-delta, 0, d * delta);
            }
            else if (position == 29)
            {
                return new Vector3(0, 0, max);
            }
            else if (position <= 34)
            {
                var d = position - 30;
                return new Vector3(delta, 0, max - d * delta);
            }
            else if (position <= 38)
            {
                var d = position - 34 + 1;
                return new Vector3(d * delta, 0, delta);
            }
            else if (position == 39)
            {
                return new Vector3(max, 0, 0);
            }
            else
            {
                // error
                return new Vector3(0, 0, 0);
            }
        }
    }
}
