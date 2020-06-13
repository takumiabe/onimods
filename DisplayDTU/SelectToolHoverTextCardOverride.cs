using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using STRINGS;

namespace DisplayDTU
{
    internal class SelectToolHoverTextCardOverride
    {
        private SelectToolHoverTextCard __this;
        private SelectToolHoverTextCardAccessor __private_accessor;

        internal SelectToolHoverTextCardOverride(SelectToolHoverTextCard instance)
        {
            this.__this = instance;
            this.__private_accessor = new SelectToolHoverTextCardAccessor(instance);
        }

        // 本体
        internal void UpdateHoverElements(List<KSelectable> hoverObjects)
        {
            if (__this.iconWarning == null)
            {
                __this.ConfigureHoverScreen();
            }
            if (OverlayScreen.Instance == null || !Grid.IsValidCell(cellPos))
            {
                return;
            }

            HoverTextDrawer hoverTextDrawer = HoverTextScreen.Instance.BeginDrawing();

            __private_accessor.overlayValidHoverObjects.Clear();
            foreach (KSelectable kselectable in hoverObjects)
            {
                if (__private_accessor.ShouldShowSelectableInCurrentOverlay(kselectable))
                {
                    __private_accessor.overlayValidHoverObjects.Add(kselectable);
                }
            }

            __this.currentSelectedSelectableIndex = -1;
            if (SelectToolHoverTextCard.highlightedObjects.Count > 0)
            {
                SelectToolHoverTextCard.highlightedObjects.Clear();
            }

            if (CurrentMode == OverlayModes.Temperature.ID && Game.Instance.temperatureOverlayMode == Game.TemperatureOverlayModes.HeatFlow)
            {
                HeatflowCard(hoverTextDrawer);
            }
            else if (CurrentMode == OverlayModes.Decor.ID)
            {
                DecorCard(hoverTextDrawer);
            }
            else if (CurrentMode == OverlayModes.Rooms.ID)
            {
                RoomCard(hoverTextDrawer);
            }
            else if (CurrentMode == OverlayModes.Light.ID)
            {
                    LightingCard(hoverTextDrawer);
            }
            else if (CurrentMode == OverlayModes.Logic.ID)
            {
                LogicCard(hoverTextDrawer, hoverObjects);
            }

            int numOfSelectableCard = 0;

            // kを使いたいのでforeachではなくfor
            for (int k = 0; k < __private_accessor.overlayValidHoverObjects.Count; k++)
            {

                KSelectable kselectable3 = __private_accessor.overlayValidHoverObjects[k];
                if (kselectable3 == null) continue;
                if (kselectable3.GetComponent<CellSelectionObject>() != null) continue;

                // if ((!(OverlayScreen.Instance != null) || !(OverlayScreen.Instance.mode != OverlayModes.None.ID) || (kselectable3.gameObject.layer & __private_accessor.maskOverlay) == 0) && isVisible)
                if (!isVisible) continue;
                if (!(OverlayScreen.Instance != null) || !(OverlayScreen.Instance.mode != OverlayModes.None.ID) || (kselectable3.gameObject.layer & __private_accessor.maskOverlay) == 0)
                {
                }
                else
                {
                    continue;
                }


                bool selected = SelectTool.Instance.selected == kselectable3;
                if (selected)
                {
                    __this.currentSelectedSelectableIndex = k;
                }
                numOfSelectableCard++;

                ItemCard(hoverTextDrawer, kselectable3, selected);
            }
            __this.recentNumberOfDisplayedSelectables = numOfSelectableCard + 1;

            if (displayMiscCard)
            {
                MiscCard(hoverTextDrawer);
            }
            else if (!isVisible)
            {
                hoverTextDrawer.BeginShadowBar(false);
                hoverTextDrawer.DrawIcon(__this.iconWarning, 18);
                hoverTextDrawer.DrawText(UI.TOOLS.GENERIC.UNKNOWN, __this.Styles_BodyText.Standard);
                hoverTextDrawer.EndShadowBar();
            }
            hoverTextDrawer.EndDrawing();
        }

        private void HeatflowCard(HoverTextDrawer hoverTextDrawer)
        {
            if (!Grid.Solid[cellPos] && isVisible)
            {
                float thermalComfort = GameUtil.GetThermalComfort(cellPos, 0f);
                float thermalComfort2 = GameUtil.GetThermalComfort(cellPos, -0.08368001f);
                float num2 = 0f;

                string text = String.Empty;

                if (thermalComfort2 * 0.001f > -0.278933346f - num2 && thermalComfort2 * 0.001f < 0.278933346f + num2)
                {
                    text = UI.OVERLAYS.HEATFLOW.NEUTRAL;
                }
                else if (thermalComfort2 <= ExternalTemperatureMonitor.GetExternalColdThreshold(null))
                {
                    text = UI.OVERLAYS.HEATFLOW.COOLING;
                }
                else if (thermalComfort2 >= ExternalTemperatureMonitor.GetExternalWarmThreshold(null))
                {
                    text = UI.OVERLAYS.HEATFLOW.HEATING;
                }
                float dtu_s = 1f * thermalComfort;
                text = text + " (" + GameUtil.GetFormattedHeatEnergyRate(dtu_s, GameUtil.HeatEnergyFormatterUnit.Automatic) + ")";
                hoverTextDrawer.BeginShadowBar(false);
                hoverTextDrawer.DrawText(UI.OVERLAYS.HEATFLOW.HOVERTITLE, __this.Styles_BodyText.Standard);
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawText(text, __this.Styles_BodyText.Standard);
                hoverTextDrawer.EndShadowBar();
            }
        }

        private void DecorCardDetail(HoverTextDrawer hoverTextDrawer)
        {
            List<EffectorEntry> positiveEffector = new List<EffectorEntry>();
            List<EffectorEntry> negativeEffector = new List<EffectorEntry>();

            // Decorの影響を集計する。同時に、影響を与えたもののハイライト表示をする
            List<DecorProvider> list = new List<DecorProvider>();
            GameScenePartitioner.Instance.TriggerEvent(cellPos, GameScenePartitioner.Instance.decorProviderLayer, list);
            foreach (DecorProvider decorProvider in list)
            {
                float decorForCell = decorProvider.GetDecorForCell(cellPos);
                if (decorForCell == 0f) continue;

                string decorProviderName = decorProvider.GetName();

                // Decorに関係するオブジェクトをハイライト表示する
                KMonoBehaviour component = decorProvider.GetComponent<KMonoBehaviour>();
                if (component != null && component.gameObject != null)
                {
                    SelectToolHoverTextCard.highlightedObjects.Add(component.gameObject);

                    // 完成したモニュメントの場合
                    if (component.GetComponent<MonumentPart>() != null && component.GetComponent<MonumentPart>().IsMonumentCompleted())
                    {
                        // 名前をoverrideする
                        decorProviderName = MISC.MONUMENT_COMPLETE.NAME;
                        // 連結したものも全てハイライトする
                        SelectToolHoverTextCard.highlightedObjects.AddRange(
                            AttachableBuilding.GetAttachedNetwork(component.GetComponent<AttachableBuilding>())
                        );
                    }
                }

                List<EffectorEntry> pos_or_neg = (decorForCell > 0f) ? positiveEffector : negativeEffector;
                EffectorEntry effector = pos_or_neg.FirstOrDefault(e => e.name == decorProviderName);
                if (effector == null)
                {
                    effector = new EffectorEntry(decorProviderName);
                    pos_or_neg.Add(effector);
                }
                effector.count++;
                effector.value += decorForCell;
            }

            // 光源効果
            int lightDecorBonus = DecorProvider.GetLightDecorBonus(cellPos);
            if (lightDecorBonus > 0)
            {
                positiveEffector.Add(new EffectorEntry(UI.OVERLAYS.DECOR.LIGHTING, (float)lightDecorBonus));
            }

            if (positiveEffector.Count > 0)
            {
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawText(UI.OVERLAYS.DECOR.HEADER_POSITIVE, __this.Styles_BodyText.Standard);
            }
            foreach (EffectorEntry entry in positiveEffector.OrderByDescending(e => e.value))
            {
                hoverTextDrawer.NewLine(18);
                DecorCardEntry(hoverTextDrawer, entry);
            }

            if (negativeEffector.Count > 0)
            {
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawText(UI.OVERLAYS.DECOR.HEADER_NEGATIVE, __this.Styles_BodyText.Standard);
            }
            foreach (EffectorEntry entry in negativeEffector.OrderBy(e => e.value))
            {
                hoverTextDrawer.NewLine(18);
                DecorCardEntry(hoverTextDrawer, entry);
            }
        }

        // EffectorEntry
        private void DecorCardEntry(HoverTextDrawer hoverTextDrawer, EffectorEntry entry)
        {
            string text = string.Empty;
            if (entry.count > 1)
            {
                text = string.Format(UI.OVERLAYS.DECOR.COUNT, entry.count);
            }
            text = string.Format(UI.OVERLAYS.DECOR.ENTRY, GameUtil.GetFormattedDecor(entry.value, false), entry.name, text);

            hoverTextDrawer.DrawIcon(__this.iconDash, 18);
            hoverTextDrawer.DrawText(text, __this.Styles_BodyText.Standard);
        }

        private void DecorCard(HoverTextDrawer hoverTextDrawer)
        {
            float decorAtCell = GameUtil.GetDecorAtCell(cellPos);
            hoverTextDrawer.BeginShadowBar(false);
            hoverTextDrawer.DrawText(UI.OVERLAYS.DECOR.HOVERTITLE, __this.Styles_BodyText.Standard);
            hoverTextDrawer.NewLine(26);
            hoverTextDrawer.DrawText(UI.OVERLAYS.DECOR.TOTAL + GameUtil.GetFormattedDecor(decorAtCell, true), __this.Styles_BodyText.Standard);

            if (!Grid.Solid[cellPos] && isVisible)
            {
                DecorCardDetail(hoverTextDrawer);
            }
            hoverTextDrawer.EndShadowBar();
        }

        private void RoomCard(HoverTextDrawer hoverTextDrawer)
        {
            CavityInfo cavityForCell = Game.Instance.roomProber.GetCavityForCell(cellPos);
            if (cavityForCell == null) return;

            Room room = cavityForCell.room;
            RoomType roomType = room?.roomType;

            hoverTextDrawer.BeginShadowBar(false);

            string roomTypeName = roomType?.Name ?? UI.OVERLAYS.ROOMS.NOROOM.HEADER;
            hoverTextDrawer.DrawText(roomTypeName, __this.Styles_BodyText.Standard);

            if (room != null)
            {
                string roomEffect = RoomDetails.EFFECT.resolve_string_function.Invoke(room);
                string roomAssignee = RoomDetails.ASSIGNED_TO.resolve_string_function.Invoke(room);
                string roomCriteria = RoomConstraints.RoomCriteriaString(room);
                string roomEffects = RoomDetails.EFFECTS.resolve_string_function.Invoke(room);

                if (roomEffect != "")
                {
                    hoverTextDrawer.NewLine(26);
                    hoverTextDrawer.DrawText(roomEffect, __this.Styles_BodyText.Standard);
                }

                if (roomAssignee != "" && roomType != Db.Get().RoomTypes.Neutral)
                {
                    hoverTextDrawer.NewLine(26);
                    hoverTextDrawer.DrawText(roomAssignee, __this.Styles_BodyText.Standard);
                }

                hoverTextDrawer.NewLine(22);
                hoverTextDrawer.DrawText(RoomDetails.RoomDetailString(room), __this.Styles_BodyText.Standard);

                if (roomCriteria != "")
                {
                    hoverTextDrawer.NewLine(26);
                    hoverTextDrawer.DrawText(roomCriteria, __this.Styles_BodyText.Standard);
                }
                if (roomEffects != "")
                {
                    hoverTextDrawer.NewLine(26);
                    hoverTextDrawer.DrawText(roomEffects, __this.Styles_BodyText.Standard);
                }
            }
            else
            {
                string roomNA = UI.OVERLAYS.ROOMS.NOROOM.DESC;
                int maxRoomSize = TuningData<RoomProber.Tuning>.Get().maxRoomSize;
                if (cavityForCell.numCells > maxRoomSize)
                {
                    roomNA = roomNA + "\n" + string.Format(UI.OVERLAYS.ROOMS.NOROOM.TOO_BIG, cavityForCell.numCells, maxRoomSize);
                }
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawText(roomNA, __this.Styles_BodyText.Standard);
            }
            hoverTextDrawer.EndShadowBar();
        }

        private void LightingCard(HoverTextDrawer hoverTextDrawer)
        {
            if (!isVisible)
            {
                return;
            }

            string text = string.Concat(new string[]
            {
                "",
                string.Format(UI.OVERLAYS.LIGHTING.DESC, Grid.LightIntensity[cellPos]),
                " (",
                GameUtil.GetLightDescription(Grid.LightIntensity[cellPos]),
                ")"
            });
            hoverTextDrawer.BeginShadowBar(false);
            hoverTextDrawer.DrawText(UI.OVERLAYS.LIGHTING.HOVERTITLE, __this.Styles_BodyText.Standard);
            hoverTextDrawer.NewLine(26);
            hoverTextDrawer.DrawText(text, __this.Styles_BodyText.Standard);
            hoverTextDrawer.EndShadowBar();
        }

        private void LogicCardPort(HoverTextDrawer hoverTextDrawer, string properName, LogicPorts ports, LogicPorts.Port port, bool isInput)
        {
            hoverTextDrawer.BeginShadowBar(false);

            LocString defaultName;
            int portState;
            string fmt;
            if (isInput)
            {
                defaultName = UI.LOGIC_PORTS.PORT_INPUT_DEFAULT_NAME;
                portState = ports.GetInputValue(port.id);
                fmt = UI.TOOLS.GENERIC.LOGIC_INPUT_HOVER_FMT;
            }
            else
            {
                defaultName = UI.LOGIC_PORTS.PORT_OUTPUT_DEFAULT_NAME;
                portState = ports.GetOutputValue(port.id);
                fmt = UI.TOOLS.GENERIC.LOGIC_OUTPUT_HOVER_FMT;
            }
            string desc = port.displayCustomName ? port.description : defaultName.text;
            hoverTextDrawer.DrawText(fmt.Replace("{Port}", desc.ToUpper()).Replace("{Name}", properName), __this.Styles_BodyText.Standard);
            hoverTextDrawer.NewLine(26);

            bool portConnected = ports.IsPortConnected(port.id);
            TextStyleSetting textStyleSetting;
            if (portConnected)
            {
                textStyleSetting = ((portState == 1) ? __this.Styles_LogicActive.Selected : __this.Styles_LogicSignalInactive);
            }
            else
            {
                textStyleSetting = __this.Styles_LogicActive.Standard;
            }
            hoverTextDrawer.DrawIcon((portState == 1 && portConnected) ? __this.iconActiveAutomationPort : __this.iconDash, textStyleSetting.textColor, 18, 2);
            hoverTextDrawer.DrawText(port.activeDescription, textStyleSetting);
            hoverTextDrawer.NewLine(26);

            TextStyleSetting textStyleSetting2;
            if (portConnected)
            {
                textStyleSetting2 = ((portState == 0) ? __this.Styles_LogicStandby.Selected : __this.Styles_LogicSignalInactive);
            }
            else
            {
                textStyleSetting2 = __this.Styles_LogicStandby.Standard;
            }
            hoverTextDrawer.DrawIcon((portState == 0 && portConnected) ? __this.iconActiveAutomationPort : __this.iconDash, textStyleSetting2.textColor, 18, 2);
            hoverTextDrawer.DrawText(port.inactiveDescription, textStyleSetting2);
            hoverTextDrawer.EndShadowBar();

        }

        private void LogicCardGate(HoverTextDrawer hoverTextDrawer, string properName, LogicGate gates, LogicGateBase.PortId portId)
        {
            int portValue = gates.GetPortValue(portId);
            bool portConnected = gates.GetPortConnected(portId);
            LogicGate.LogicGateDescriptions.Description portDescription = gates.GetPortDescription(portId);
            hoverTextDrawer.BeginShadowBar(false);

            LocString fmt =
                portId == LogicGateBase.PortId.Output ?
                UI.TOOLS.GENERIC.LOGIC_MULTI_OUTPUT_HOVER_FMT :
                UI.TOOLS.GENERIC.LOGIC_MULTI_INPUT_HOVER_FMT;
            hoverTextDrawer.DrawText(fmt.Replace("{Port}", portDescription.name.ToUpper()).Replace("{Name}", properName), __this.Styles_BodyText.Standard);


            hoverTextDrawer.NewLine(26);
            TextStyleSetting textStyleSetting3;
            if (portConnected)
            {
                textStyleSetting3 = ((portValue == 1) ? __this.Styles_LogicActive.Selected : __this.Styles_LogicSignalInactive);
            }
            else
            {
                textStyleSetting3 = __this.Styles_LogicActive.Standard;
            }
            hoverTextDrawer.DrawIcon((portValue == 1 && portConnected) ? __this.iconActiveAutomationPort : __this.iconDash, textStyleSetting3.textColor, 18, 2);
            hoverTextDrawer.DrawText(portDescription.active, textStyleSetting3);
            hoverTextDrawer.NewLine(26);
            TextStyleSetting textStyleSetting4;
            if (portConnected)
            {
                textStyleSetting4 = ((portValue == 0) ? __this.Styles_LogicStandby.Selected : __this.Styles_LogicSignalInactive);
            }
            else
            {
                textStyleSetting4 = __this.Styles_LogicStandby.Standard;
            }
            hoverTextDrawer.DrawIcon((portValue == 0 && portConnected) ? __this.iconActiveAutomationPort : __this.iconDash, textStyleSetting4.textColor, 18, 2);
            hoverTextDrawer.DrawText(portDescription.inactive, textStyleSetting4);
            hoverTextDrawer.EndShadowBar();
        }

        private void LogicCard(HoverTextDrawer hoverTextDrawer, List<KSelectable> hoverObjects)
        {
            foreach (KSelectable kselectable2 in hoverObjects)
            {
                LogicPorts ports = kselectable2.GetComponent<LogicPorts>();
                string properName = kselectable2.GetProperName().ToUpper();

                if (ports != null)
                {
                    LogicPorts.Port port;
                    bool portIsInput;
                    if (ports.TryGetPortAtCell(cellPos, out port, out portIsInput))
                    {
                        LogicCardPort(hoverTextDrawer, properName, ports, port, portIsInput);
                    }

                }

                LogicGate gates = kselectable2.GetComponent<LogicGate>();
                LogicGateBase.PortId portId;
                if (gates != null && gates.TryGetPortAtCell(cellPos, out portId))
                {
                    LogicCardGate(hoverTextDrawer, properName, gates, portId);
                }
            }
        }

        private void MiscCardDisease(HoverTextDrawer hoverTextDrawer)
        {
            if (Grid.DiseaseCount[cellPos] > 0 || modeIsDisease)
            {
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawIcon(__this.iconDash, 18);
                hoverTextDrawer.DrawText(GameUtil.GetFormattedDisease(Grid.DiseaseIdx[cellPos], Grid.DiseaseCount[cellPos], true), __this.Styles_Values.Property.Standard);
            }
        }

        private void MiscCardElementCategory(HoverTextDrawer hoverTextDrawer)
        {
            Element element = Grid.Element[cellPos];
            if (!element.IsVacuum)
            {
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawIcon(__this.iconDash, 18);
                hoverTextDrawer.DrawText(ElementLoader.elements[(int)Grid.ElementIdx[cellPos]].GetMaterialCategoryTag().ProperName(), __this.Styles_BodyText.Standard);
            }
        }

        private void MiscCardMass(HoverTextDrawer hoverTextDrawer)
        {
            Element element = Grid.Element[cellPos];

            string[] array = WorldInspector.MassStringsReadOnly(cellPos);
            hoverTextDrawer.NewLine(26);
            hoverTextDrawer.DrawIcon(__this.iconDash, 18);
            // キャッシュ済みのGetBreathableString的なモノを出す。
            for (int m = 0; m < array.Length; m++)
            {
                if (m >= 3 || !element.IsVacuum)
                {
                    hoverTextDrawer.DrawText(array[m], __this.Styles_BodyText.Standard);
                }
            }
        }

        private void MiscCardTemperature(HoverTextDrawer hoverTextDrawer)
        {
            Element element = Grid.Element[cellPos];
            if (!element.IsVacuum)
            {
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawIcon(__this.iconDash, 18);
                string formattedTemperature = __private_accessor.cachedTemperatureString;
                float currentTemp = Grid.Temperature[cellPos];
                if (currentTemp != __private_accessor.cachedTemperature)
                {
                    __private_accessor.cachedTemperature = currentTemp;
                    formattedTemperature = GameUtil.GetFormattedTemperature(Grid.Temperature[cellPos], GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Absolute, true, false);
                    __private_accessor.cachedTemperatureString = formattedTemperature;
                }
                string text14 = (element.specificHeatCapacity == 0f) ? "N/A" : formattedTemperature;

                hoverTextDrawer.DrawText(text14, __this.Styles_BodyText.Standard);

                float dtu = currentTemp * element.specificHeatCapacity * Grid.Mass[cellPos];
                DrawKiloDTU(hoverTextDrawer, dtu);
            }
        }

        private void MiscCardExposedToSpace(HoverTextDrawer hoverTextDrawer)
        {
            if (CellSelectionObject.IsExposedToSpace(cellPos))
            {
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawIcon(__this.iconDash, 18);
                hoverTextDrawer.DrawText(MISC.STATUSITEMS.SPACE.NAME, __this.Styles_BodyText.Standard);
            }
        }

        private void MiscCardEntombedItem(HoverTextDrawer hoverTextDrawer)
        {
            if (Game.Instance.GetComponent<EntombedItemVisualizer>().IsEntombedItem(cellPos))
            {
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawIcon(__this.iconDash, 18);
                hoverTextDrawer.DrawText(MISC.STATUSITEMS.BURIEDITEM.NAME, __this.Styles_BodyText.Standard);
            }
        }

        private void MiscCardOxyRock(HoverTextDrawer hoverTextDrawer)
        {
            Element element = Grid.Element[cellPos];
            if (element.id == SimHashes.OxyRock)
            {
                float num7 = Grid.AccumulatedFlow[cellPos] / 3f;
                string template = BUILDING.STATUSITEMS.EMITTINGOXYGENAVG.NAME;
                string oxyflow_str = template.Replace("{FlowRate}", GameUtil.GetFormattedMass(num7, GameUtil.TimeSlice.PerSecond, GameUtil.MetricMassFormat.UseThreshold, true, "{0:0.#}"));
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawIcon(__this.iconDash, 18);
                hoverTextDrawer.DrawText(oxyflow_str, __this.Styles_BodyText.Standard);
                if (num7 <= 0f)
                {
                    bool not_in_gas;
                    bool overpressure;
                    GameUtil.IsEmissionBlocked(cellPos, out not_in_gas, out overpressure);

                    string cause_str = null;
                    if (not_in_gas)
                    {
                        cause_str = MISC.STATUSITEMS.OXYROCK.NEIGHBORSBLOCKED.NAME;
                    }
                    else if (overpressure)
                    {
                        cause_str = MISC.STATUSITEMS.OXYROCK.OVERPRESSURE.NAME;
                    }
                    if (cause_str != null)
                    {
                        hoverTextDrawer.NewLine(26);
                        hoverTextDrawer.DrawIcon(__this.iconDash, 18);
                        hoverTextDrawer.DrawText(cause_str, __this.Styles_BodyText.Standard);
                    }
                }
            }

        }

        private void MiscCardElement(HoverTextDrawer hoverTextDrawer)
        {
            Element element = Grid.Element[cellPos];
            hoverTextDrawer.DrawText(element.nameUpperCase, __this.Styles_BodyText.Standard);
        }

        private void MiscCard(HoverTextDrawer hoverTextDrawer)
        {
            CellSelectionObject cellSelectionObject = this.cellSelectionObject;
            bool flag12 = cellSelectionObject != null && cellSelectionObject.mouseCell == cellSelectionObject.alternateSelectionObject.mouseCell;
            if (flag12)
            {
                __this.currentSelectedSelectableIndex = __this.recentNumberOfDisplayedSelectables - 1;
            }
            hoverTextDrawer.BeginShadowBar(flag12);

            MiscCardElement(hoverTextDrawer);
            MiscCardDisease(hoverTextDrawer);
            MiscCardElementCategory(hoverTextDrawer);
            MiscCardMass(hoverTextDrawer);
            MiscCardTemperature(hoverTextDrawer);
            MiscCardExposedToSpace(hoverTextDrawer);
            MiscCardEntombedItem(hoverTextDrawer);
            MiscCardOxyRock(hoverTextDrawer);

            hoverTextDrawer.EndShadowBar();
        }

        private void ItemCardDisease(HoverTextDrawer hoverTextDrawer, KSelectable selectable)
        {
            if (!modeIsDisease) return;

            PrimaryElement primaryElement = selectable.GetComponent<PrimaryElement>();

            string text13 = UI.OVERLAYS.DISEASE.NO_DISEASE;
            if (primaryElement != null && primaryElement.DiseaseIdx != 255)
            {
                text13 = GameUtil.GetFormattedDisease(primaryElement.DiseaseIdx, primaryElement.DiseaseCount, true);
            }
            Storage storage = selectable.GetComponent<Storage>();
            if (storage != null && storage.showInUI)
            {
                foreach (GameObject item in storage.items)
                {
                    if (item == null) continue;
                    PrimaryElement itemElement = item.GetComponent<PrimaryElement>();
                    if (itemElement.DiseaseIdx != 255)
                    {
                        text13 += string.Format(
                            UI.OVERLAYS.DISEASE.CONTAINER_FORMAT,
                            item.GetComponent<KSelectable>().GetProperName(),
                            GameUtil.GetFormattedDisease(itemElement.DiseaseIdx, itemElement.DiseaseCount, true)
                        );
                    }
                }
            }
            hoverTextDrawer.NewLine(26);
            hoverTextDrawer.DrawIcon(__this.iconDash, 18);
            hoverTextDrawer.DrawText(text13, __this.Styles_Values.Property.Standard);
        }


        private void ItemCardStatus(HoverTextDrawer hoverTextDrawer, KSelectable kselectable3)
        {
            // 何故IEnumerable<Entry>を宣言してないのか？？？
            List<StatusItemGroup.Entry> entries = new List<StatusItemGroup.Entry>();
            foreach (StatusItemGroup.Entry entry in kselectable3.GetStatusItemGroup())
            {
                entries.Add(entry);
            }

            var a = entries.
                Where((e) => __private_accessor.ShowStatusItemInCurrentOverlay(e.item)).
                OrderBy((e) => e.category != null && e.category.Id != "Main" ? 0 : 1).
                Take(SelectToolHoverTextCard.maxNumberOfDisplayedSelectableWarnings);

            foreach (StatusItemGroup.Entry entry in entries)
            {
                Sprite icon = entry.item.sprite?.sprite ?? __this.iconWarning;

                Color color = __this.Styles_BodyText.Standard.textColor;
                TextStyleSetting style = __this.Styles_BodyText.Standard;
                if (__private_accessor.IsStatusItemWarning(entry))
                {
                    color = __this.HoverTextStyleSettings[1].textColor;
                    style = __this.HoverTextStyleSettings[1];
                }

                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawIcon(icon, color, 18, 2);
                hoverTextDrawer.DrawText(entry.GetName(), style);
            }
        }

        private void ItemCard(HoverTextDrawer hoverTextDrawer, KSelectable kselectable3, bool selected)
        {
            hoverTextDrawer.BeginShadowBar(selected);

            ItemCardBasic(hoverTextDrawer, kselectable3);
            ItemCardDisease(hoverTextDrawer, kselectable3);
            ItemCardStatus(hoverTextDrawer, kselectable3);
            ItemCardThermal(hoverTextDrawer, kselectable3);
            ItemCardLightingForChore(hoverTextDrawer, kselectable3);

            hoverTextDrawer.EndShadowBar();
        }

        private void ItemCardBasic(HoverTextDrawer hoverTextDrawer, KSelectable kselectable3)
        {
            PrimaryElement primaryElement = kselectable3.GetComponent<PrimaryElement>();
            string text12 = GameUtil.GetUnitFormattedName(kselectable3.gameObject, true);

            if (primaryElement != null && kselectable3.GetComponent<Building>() != null)
            {
                text12 = StringFormatter.Replace(StringFormatter.Replace(UI.TOOLS.GENERIC.BUILDING_HOVER_NAME_FMT, "{Name}", text12), "{Element}", primaryElement.Element.nameUpperCase);
            }
            hoverTextDrawer.DrawText(text12, __this.Styles_BodyText.Standard);
        }

        private void ItemCardThermal(HoverTextDrawer hoverTextDrawer, KSelectable kselectable3)
        {
            if (kselectable3.GetComponent<Constructable>())
            {
                return;
            }
            if (CurrentMode != OverlayModes.None.ID && CurrentMode != OverlayModes.Temperature.ID)
            {
                return;
            }
            // None か Temperatureオーバーレイでのみ表示

            PrimaryElement primaryElement = kselectable3.GetComponent<PrimaryElement>();
            float temp = 0f;
            float dtu = -1f;
            bool flag10 = CurrentMode == OverlayModes.Temperature.ID && Game.Instance.temperatureOverlayMode != Game.TemperatureOverlayModes.HeatFlow;

            if (flag10 && primaryElement)
            {
                temp = primaryElement.Temperature;
                dtu = temp * primaryElement.Element.specificHeatCapacity * primaryElement.Mass;
            }
            else if (kselectable3.GetComponent<Building>() && primaryElement)
            {
                temp = primaryElement.Temperature;
                dtu = temp * primaryElement.Element.specificHeatCapacity * primaryElement.Mass;
            }
            else if (kselectable3.GetComponent<CellSelectionObject>() != null)
            {
                CellSelectionObject obj = kselectable3.GetComponent<CellSelectionObject>();
                temp = obj.temperature;
                dtu = temp * obj.element.specificHeatCapacity * obj.Mass;
            }
            else
            {
                return;
            }

            hoverTextDrawer.NewLine(26);
            hoverTextDrawer.DrawIcon(__this.iconDash, 18);
            hoverTextDrawer.DrawText(GameUtil.GetFormattedTemperature(temp, GameUtil.TimeSlice.None, GameUtil.TemperatureInterpretation.Absolute, true, false), __this.Styles_BodyText.Standard);

            DrawKiloDTU(hoverTextDrawer, dtu);
        }

        private void ItemCardLightingForChore(HoverTextDrawer hoverTextDrawer, KSelectable kSelectable3)
        {
            // 照明オーバーレイにおいて、照明効果を表示する
            if (CurrentMode != OverlayModes.Light.ID) return;
            if (choreConsumer == null) return;

            bool toHidden = __private_accessor.hiddenChoreConsumerTypes.Any(type => choreConsumer.gameObject.GetComponent(type) != null);
            if (toHidden) return;

            choreConsumer.ShowHoverTextOnHoveredItem(kSelectable3, hoverTextDrawer, __private_accessor.original);
        }

        private void DrawKiloDTU(HoverTextDrawer hoverTextDrawer, float dtu)
        {
            if (CurrentMode == OverlayModes.Temperature.ID && dtu > 0.0f)
            {
                hoverTextDrawer.NewLine(26);
                hoverTextDrawer.DrawIcon(__this.iconDash, 18);
                hoverTextDrawer.DrawText(Mathf.Round(dtu / 100.0f) / 10.0f + "k DTU", __this.Styles_BodyText.Standard);
            }
        }

        private int cellPos
        {
            get
            {
                return Grid.PosToCell(Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos()));
            }
        }

        private bool isVisible
        {
            get
            {
                return Grid.IsVisible(cellPos);
            }
        }

        private bool displayMiscCard
        {
            get
            {
                if (Grid.DupePassable[cellPos])
                {
                    return false;
                }
                if (!isVisible)
                {
                    return false;
                }
                foreach (KeyValuePair<HashedString, Func<bool>> keyValuePair in __private_accessor.overlayFilterMap)
                {
                    if (OverlayScreen.Instance.GetMode() == keyValuePair.Key)
                    {
                        if (!keyValuePair.Value.Invoke())
                        {
                            return false;
                        }
                        break;
                    }
                }
                // 選択要素の中に間欠泉が含まれていたらMiscCardは表示しない
                foreach (KSelectable k in __private_accessor.overlayValidHoverObjects)
                {
                    BuildingComplete component7 = k.GetComponent<BuildingComplete>();
                    if (component7 != null && component7.Def.IsFoundation)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private ChoreConsumer choreConsumer
        {
            get
            {
                // nullBehaviorかもしれないので ?. を使ってはいけない
                if (SelectTool.Instance.selected == null) return null;
                return SelectTool.Instance.selected.GetComponent<ChoreConsumer>();
            }
        }
        private CellSelectionObject cellSelectionObject
        {
            get
            {
                if (SelectTool.Instance.selected == null) return null;
                return SelectTool.Instance.selected.GetComponent<CellSelectionObject>();
            }
        }
        private HashedString CurrentMode
        {
            get
            {
                return SimDebugView.Instance.GetMode();
            }
        }
        private bool modeIsDisease
        {
            get
            {
                return CurrentMode == OverlayModes.Disease.ID;

            }
        }
    }
}
