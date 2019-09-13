using GameFramework.GameStructure.Worlds.ObjectModel;
using UnityEngine;

namespace GameFrameworkTutorials.GettingStarted.Scripts
{
    [CreateAssetMenu(fileName = "World_x", menuName = "Game Framework/Custom World")]

    public class CustomWorld : World
    {

        public string Difficulty;
    }
}