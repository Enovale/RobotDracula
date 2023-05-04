using System.Collections.Generic;

namespace RobotDracula.ChoiceEvent.Automation
{
    public static partial class ChoiceEventAutomation
    {
        private static Dictionary<int, int> _choiceActionDict = new()
        {
            {900011, 0}, // Vending Machine
            {900012, 0},
            {900013, 0},
            {900021, 0}, // New Recruit
            {900031, 0}, // Power Up
            {901017, 0}, // Judgement Tablet - Read the tablet.
            {901013, 0}, // Wooden Doll - Remove Doll Talismans
            {901022, 0}, // Empty Statue - Grab it's hand
            {901021, 0}, // Passenger between dimensions - Point where it should go
            {901011, 1}, // Umbrella Fox - Pet the fox
            {901005, 0}, // Coffin - Open Coffin
            {901008, 0}, // Teddy Bear - Remove the Nails
            {901006, 1}, // Business Crane - Shake Your Head
            {901027, 1}, // Blue Star - Backward
            {901003, 0}, // Ghostly Mist - Walk into the mist
            {901002, 1}, // The Land of Gold - Avert your eyes
            {901032, 0}, // Electric Sheep - Cut the cables
            {901034, 0}, // KQE Part One - Say [Hello]
            {90103401, 1}, // KQE Part Two - Take it and leave.
            {901035, 1}, // Scissors Girl - Play paper
            {910302, 0}, // Chicken Tango
        };
    }
}