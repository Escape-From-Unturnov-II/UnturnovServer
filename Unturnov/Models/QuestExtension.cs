using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models
{
    /*
        Dialogue:

        Response_2_Dialogue 57098
        Response_2_Quest 57022
        Response_2_Conditions 1
        Response_2_Condition_0_Type Flag_Short
        Response_2_Condition_0_ID 55218
        Response_2_Condition_0_Allow_Unset

        Response_2_Rewards 2
        Response_2_Reward_0_Type Flag_Short
        Response_2_Reward_0_ID 55218
        Response_2_Reward_0_Value 0
        Response_2_Reward_0_Modification Assign
        Response_2_Reward_1_Type Quest
        Response_2_Reward_1_ID 57022

        Quest:

        Condition_0_Type Flag_Short
        Condition_0_ID 55218
        Condition_0_Value 1
        Condition_0_Logic Greater_Than_Or_Equal_To

        Quest Text:

        Name <color=rare>Kill Test</color>
        Description Kill a Player with a Makarov

        Condition_0 Killed {0}/{1} Players with Makarov
    */
    public class QuestExtension
    {
        public ushort QuestFlagId;
        public bool NoGun;
        public bool NoVest;
        public bool NoHat;
        public bool NoBackpack;
        public List<string> EquipmentIdentifiers;
    }
}
