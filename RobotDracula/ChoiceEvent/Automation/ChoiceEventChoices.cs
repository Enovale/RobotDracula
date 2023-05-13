using System.Collections.Generic;

namespace RobotDracula.ChoiceEvent.Automation
{
    public static partial class ChoiceEventAutomation
    {
        private static Dictionary<int, int> _choiceActionDict = new()
        {
            // Mirror Dungeon
            {900011, 0}, // Vending Machine
            {900012, 0},
            {900013, 0},
            {900021, 0}, // New Recruit
            {900031, 0}, // Power Up
            
            {901002, 1}, // The Land of Gold - Avert your eyes
            {901003, 0}, // Ghostly Mist - Walk into the mist
            {901004, 0}, // Doctor - Accept Blood
            {901005, 0}, // Coffin - Open Coffin
            {901006, 1}, // Business Crane - Shake Your Head
            {901007, 0}, // Yellow Butterflies - Take a break
            {901008, 0}, // Teddy Bear - Remove the Nails
            {901009, 1}, // Fire Candle Person - Peek at the book
            {901010, 1}, // Red Jewel - Take the brass ring
            {901011, 1}, // Umbrella Fox - Pet the fox
            {901013, 0}, // Wooden Doll - Remove Doll Talismans
            {901014, 0}, // Burning Beast - Give water
            {901016, 1}, // Drink Cavern - Shake your head.
            {901017, 0}, // Judgement Tablet - Read the tablet.
            {901018, 0}, // Signboard - Pick a rose.
            {901019, 0}, // Giant Clam - Sample the green substance.
            {901020, 0}, // Field of white - Approach it
            {901021, 0}, // Passenger between dimensions - Point where it should go
            {901022, 0}, // Empty Statue - Grab it's hand
            {901023, 1}, // Resentful Tree - Snap the twig
            {901024, 1}, // Croohoo - Sit and wait
            {901025, 0}, // Person stands in the Square - Approach Them
            {901026, 0}, // Cluster of eyes - Close your eyes
            {901027, 1}, // Blue Star - Backward
            {901028, 0}, // Flowers and Brambles - Pick a Flower
            {901031, 0}, // Machines exist for a purpose - Carry Luggage
            {901032, 0}, // Electric Sheep - Cut the cables
            {901033, 0}, // Electric Centipede - Press the lightning shaped button
            {901034, 0}, // KQE Part One - Say [Hello]
            {90103401, 1}, // KQE Part Two - Take it and leave.
            {901035, 1}, // Scissors Girl - Play paper
            
            // Chicken event
            {910301, 0}, // Chicken Sticks or Breasts - Chicken's Chicken
            {910302, 1}, // Chicken Tango - You saw nothing. - Tango Sauce
            
            // Abnormality Events
            {801501, 1}, // Pink Shoes - Refuse
            {801201, 0}, // YWTGB?H? - Investigate the factory
            {801202, 1}, // YWTGB?H? - No.
            {801203, 1}, // YWTGB?H? - No. (Doesn't appear to actually be used in-game but just in case)
            {801204, 1}, // YWTGB?H? - No.
            {800203, 0}, // Calendar - Offer Clay Doll
        };
    }
}