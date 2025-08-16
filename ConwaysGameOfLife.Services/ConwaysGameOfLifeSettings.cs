using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConwaysGameOfLife.Services
{
    /// <summary>
    /// Conway's Game of Life service settings stored in AppSettings.
    /// </summary>
    public class ConwaysGameOfLifeSettings
    {
        /// <summary>
        /// This is the maximum number of generations before the game ends preventing the game from running indefinitely.
        /// This value does not affect the number of trasitions that can be made in the game, only the number of generations
        /// executed when ending the game.
        /// </summary>
        public uint MaximunGenerationBeforeEnding { get; set; }

        /// <summary>
        /// This is the number of iterations that will be used to determine if the game has reached a stable population.
        /// Based on current studies this number should be set to 5,206 iterations.
        /// </summary>
        public uint StablePopulationIterations { get; set; }
    }
}
