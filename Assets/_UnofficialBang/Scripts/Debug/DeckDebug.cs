using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace Thirties.UnofficialBang
{
    [CreateAssetMenu(menuName = "BANG/Deck Debug")]
    public class DeckDebug : SerializedScriptableObject
    {
        [PropertySpace]
        [SerializeField]
        private CardDataTable cardDataTable;

        [PropertySpace]
        [OdinSerialize]
        [TableList]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(ListElementLabelName = "Name", HideAddButton = true, HideRemoveButton = true, ShowPaging = false, DraggableItems = true, ShowIndexLabels = true)]
        private List<CardData> roleCards;
        public List<CardData> RoleCards => roleCards;

        [PropertySpace]
        [OdinSerialize]
        [TableList]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(ListElementLabelName = "Name", HideAddButton = true, HideRemoveButton = true, ShowPaging = false, DraggableItems = true, ShowIndexLabels = true)]
        private List<CardData> characterCards;
        public List<CardData> CharacterCards => characterCards;

        [PropertySpace]
        [OdinSerialize]
        [TableList]
        [HideReferenceObjectPicker]
        [ListDrawerSettings(ListElementLabelName = "Name", HideAddButton = true, HideRemoveButton = true, ShowPaging = false, DraggableItems = true, ShowIndexLabels = true)]
        private List<CardData> mainCards;
        public List<CardData> MainCards => mainCards;

        [Button]
        private void Initialize()
        {
            mainCards = cardDataTable.GetAll()
                .Where(c => c.Class == CardClass.Blue || c.Class == CardClass.Brown)
                .ToList();

            characterCards = cardDataTable.GetAll()
                .Where(c => c.Class == CardClass.Character)
                .ToList();

            roleCards = cardDataTable.GetAll()
                .Where(c => c.Class == CardClass.Role)
                .ToList();
        }
    }
}
