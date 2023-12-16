using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedMann.Unturnov.Models.Config.QuestExtensions
{
    public class QuestCooldown
    {
        public ushort QuestId;
        public ushort MinFlag;
        public ushort SecFlag;

        public QuestCooldown()
        {

        }
        public QuestCooldown(ushort cooldownQuestId, ushort cooldownMinFlag, ushort cooldownSecFlag)
        {
            QuestId = cooldownQuestId;
            MinFlag = cooldownMinFlag;
            SecFlag = cooldownSecFlag;
        }
        public bool IsValid()
        {
            return QuestId != 0 && MinFlag != 0 && SecFlag != 0;
        }
    }
}
