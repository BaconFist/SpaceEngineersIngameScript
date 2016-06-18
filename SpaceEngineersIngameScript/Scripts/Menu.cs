using System;
using System.Collections.Generic;
using VRageMath; // VRage.Math.dll
using VRage.Game; // VRage.Game.dll
using System.Text;
using Sandbox.ModAPI.Interfaces; // Sandbox.Common.dll
using Sandbox.ModAPI.Ingame; // Sandbox.Common.dll
using Sandbox.Game.EntityComponents; // Sandbox.Game.dll
using VRage.Game.Components; // VRage.Game.dll
using VRage.Collections; // VRage.Library.dll
using VRage.Game.ObjectBuilders.Definitions; // VRage.Game.dll
using VRage.Game.ModAPI.Ingame; // VRage.Game.dll
using SpaceEngineers.Game.ModAPI.Ingame; // SpacenEngineers.Game.dll
using System.Collections;

namespace Menu
{
    public class Program : MyGridProgram
    {
        #region Game Code - Copy/Paste Code from this region into Block Script Window in Game

        /**
        Menu
        ==============
        Copyright 2016 Thomas Klose <thomas@bratler.net>
        License: https://github.com/BaconFist/SpaceEngineersIngameScript/blob/master/LICENSE

        Description
        ===========

        */

        public Program()
        {

            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.

        }

        public void Save()
        {

            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.

        }

        public void Main(string argument)
        {
            Tree.Node MenuNodes = new Tree.Node();
            MenuNodes.caption = "Menü";

            Tree.Node IMyAirVent_Nodes = new Tree.Node(MenuNodes); IMyAirVent_Nodes.caption = "AirVent";
            Tree.Node IMyAssembler_Nodes = new Tree.Node(MenuNodes); IMyAssembler_Nodes.caption = "Assembler";
            Tree.Node IMyBatteryBlock_Nodes = new Tree.Node(MenuNodes); IMyBatteryBlock_Nodes.caption = "BatteryBlock";
            Tree.Node IMyBeacon_Nodes = new Tree.Node(MenuNodes); IMyBeacon_Nodes.caption = "Beacon";
            Tree.Node IMyButtonPanel_Nodes = new Tree.Node(MenuNodes); IMyButtonPanel_Nodes.caption = "ButtonPanel";
            Tree.Node IMyCameraBlock_Nodes = new Tree.Node(MenuNodes); IMyCameraBlock_Nodes.caption = "CameraBlock";
            Tree.Node IMyCargoContainer_Nodes = new Tree.Node(MenuNodes); IMyCargoContainer_Nodes.caption = "CargoContainer";
            Tree.Node IMyCockpit_Nodes = new Tree.Node(MenuNodes); IMyCockpit_Nodes.caption = "Cockpit";
            Tree.Node IMyCollector_Nodes = new Tree.Node(MenuNodes); IMyCollector_Nodes.caption = "Collector";
            Tree.Node IMyConveyorSorter_Nodes = new Tree.Node(MenuNodes); IMyConveyorSorter_Nodes.caption = "ConveyorSorter";
            Tree.Node IMyDoor_Nodes = new Tree.Node(MenuNodes); IMyDoor_Nodes.caption = "Door";
            Tree.Node IMyGravityGenerator_Nodes = new Tree.Node(MenuNodes); IMyGravityGenerator_Nodes.caption = "GravityGenerator";
            Tree.Node IMyGyro_Nodes = new Tree.Node(MenuNodes); IMyGyro_Nodes.caption = "Gyro";
            Tree.Node IMyInteriorLight_Nodes = new Tree.Node(MenuNodes); IMyInteriorLight_Nodes.caption = "InteriorLight";
            Tree.Node IMyJumpDrive_Nodes = new Tree.Node(MenuNodes); IMyJumpDrive_Nodes.caption = "JumpDrive";
            Tree.Node IMyLandingGear_Nodes = new Tree.Node(MenuNodes); IMyLandingGear_Nodes.caption = "LandingGear";
            Tree.Node IMyLargeGatlingTurret_Nodes = new Tree.Node(MenuNodes); IMyLargeGatlingTurret_Nodes.caption = "LargeGatlingTurret";
            Tree.Node IMyLargeInteriorTurret_Nodes = new Tree.Node(MenuNodes); IMyLargeInteriorTurret_Nodes.caption = "LargeInteriorTurret";
            Tree.Node IMyLargeMissileTurret_Nodes = new Tree.Node(MenuNodes); IMyLargeMissileTurret_Nodes.caption = "LargeMissileTurret";
            Tree.Node IMyLargeTurretBase_Nodes = new Tree.Node(MenuNodes); IMyLargeTurretBase_Nodes.caption = "LargeTurretBase";
            Tree.Node IMyLaserAntenna_Nodes = new Tree.Node(MenuNodes); IMyLaserAntenna_Nodes.caption = "LaserAntenna";
            Tree.Node IMyLightingBlock_Nodes = new Tree.Node(MenuNodes); IMyLightingBlock_Nodes.caption = "LightingBlock";
            Tree.Node IMyMedicalRoom_Nodes = new Tree.Node(MenuNodes); IMyMedicalRoom_Nodes.caption = "MedicalRoom";
            Tree.Node IMyMotorAdvancedStator_Nodes = new Tree.Node(MenuNodes); IMyMotorAdvancedStator_Nodes.caption = "MotorAdvancedStator";
            Tree.Node IMyMotorBase_Nodes = new Tree.Node(MenuNodes); IMyMotorBase_Nodes.caption = "MotorBase";
            Tree.Node IMyMotorRotor_Nodes = new Tree.Node(MenuNodes); IMyMotorRotor_Nodes.caption = "MotorRotor";
            Tree.Node IMyMotorStator_Nodes = new Tree.Node(MenuNodes); IMyMotorStator_Nodes.caption = "MotorStator";
            Tree.Node IMyMotorSuspension_Nodes = new Tree.Node(MenuNodes); IMyMotorSuspension_Nodes.caption = "MotorSuspension";
            Tree.Node IMyOreDetector_Nodes = new Tree.Node(MenuNodes); IMyOreDetector_Nodes.caption = "OreDetector";
            Tree.Node IMyOxygenFarm_Nodes = new Tree.Node(MenuNodes); IMyOxygenFarm_Nodes.caption = "OxygenFarm";
            Tree.Node IMyOxygenGenerator_Nodes = new Tree.Node(MenuNodes); IMyOxygenGenerator_Nodes.caption = "OxygenGenerator";
            Tree.Node IMyOxygenTank_Nodes = new Tree.Node(MenuNodes); IMyOxygenTank_Nodes.caption = "OxygenTank";
            Tree.Node IMyPistonBase_Nodes = new Tree.Node(MenuNodes); IMyPistonBase_Nodes.caption = "PistonBase";
            Tree.Node IMyPistonTop_Nodes = new Tree.Node(MenuNodes); IMyPistonTop_Nodes.caption = "PistonTop";
            Tree.Node IMyProductionBlock_Nodes = new Tree.Node(MenuNodes); IMyProductionBlock_Nodes.caption = "ProductionBlock";
            Tree.Node IMyProgrammableBlock_Nodes = new Tree.Node(MenuNodes); IMyProgrammableBlock_Nodes.caption = "ProgrammableBlock";
            Tree.Node IMyProjector_Nodes = new Tree.Node(MenuNodes); IMyProjector_Nodes.caption = "Projector";
            Tree.Node IMyRadioAntenna_Nodes = new Tree.Node(MenuNodes); IMyRadioAntenna_Nodes.caption = "RadioAntenna";
            Tree.Node IMyReactor_Nodes = new Tree.Node(MenuNodes); IMyReactor_Nodes.caption = "Reactor";
            Tree.Node IMyRefinery_Nodes = new Tree.Node(MenuNodes); IMyRefinery_Nodes.caption = "Refinery";
            Tree.Node IMyReflectorLight_Nodes = new Tree.Node(MenuNodes); IMyReflectorLight_Nodes.caption = "ReflectorLight";
            Tree.Node IMyRemoteControl_Nodes = new Tree.Node(MenuNodes); IMyRemoteControl_Nodes.caption = "RemoteControl";
            Tree.Node IMySensorBlock_Nodes = new Tree.Node(MenuNodes); IMySensorBlock_Nodes.caption = "SensorBlock";
            Tree.Node IMyShipConnector_Nodes = new Tree.Node(MenuNodes); IMyShipConnector_Nodes.caption = "ShipConnector";
            Tree.Node IMyShipController_Nodes = new Tree.Node(MenuNodes); IMyShipController_Nodes.caption = "ShipController";
            Tree.Node IMyShipDrill_Nodes = new Tree.Node(MenuNodes); IMyShipDrill_Nodes.caption = "ShipDrill";
            Tree.Node IMyShipGrinder_Nodes = new Tree.Node(MenuNodes); IMyShipGrinder_Nodes.caption = "ShipGrinder";
            Tree.Node IMyShipMergeBlock_Nodes = new Tree.Node(MenuNodes); IMyShipMergeBlock_Nodes.caption = "ShipMergeBlock";
            Tree.Node IMyShipToolBase_Nodes = new Tree.Node(MenuNodes); IMyShipToolBase_Nodes.caption = "ShipToolBase";
            Tree.Node IMyShipWelder_Nodes = new Tree.Node(MenuNodes); IMyShipWelder_Nodes.caption = "ShipWelder";
            Tree.Node IMySlimBlock_Nodes = new Tree.Node(MenuNodes); IMySlimBlock_Nodes.caption = "SlimBlock";
            Tree.Node IMySmallGatlingGun_Nodes = new Tree.Node(MenuNodes); IMySmallGatlingGun_Nodes.caption = "SmallGatlingGun";
            Tree.Node IMySmallMissileLauncher_Nodes = new Tree.Node(MenuNodes); IMySmallMissileLauncher_Nodes.caption = "SmallMissileLauncher";
            Tree.Node IMySmallMissileLauncherReload_Nodes = new Tree.Node(MenuNodes); IMySmallMissileLauncherReload_Nodes.caption = "SmallMissileLauncherReload";
            Tree.Node IMySolarPanel_Nodes = new Tree.Node(MenuNodes); IMySolarPanel_Nodes.caption = "SolarPanel";
            Tree.Node IMySoundBlock_Nodes = new Tree.Node(MenuNodes); IMySoundBlock_Nodes.caption = "SoundBlock";
            Tree.Node IMySpaceBall_Nodes = new Tree.Node(MenuNodes); IMySpaceBall_Nodes.caption = "SpaceBall";
            Tree.Node IMyTextPanel_Nodes = new Tree.Node(MenuNodes); IMyTextPanel_Nodes.caption = "TextPanel";
            Tree.Node IMyThrust_Nodes = new Tree.Node(MenuNodes); IMyThrust_Nodes.caption = "Thrust";
            Tree.Node IMyTimerBlock_Nodes = new Tree.Node(MenuNodes); IMyTimerBlock_Nodes.caption = "TimerBlock";
            Tree.Node IMyUserControllableGun_Nodes = new Tree.Node(MenuNodes); IMyUserControllableGun_Nodes.caption = "UserControllableGun";
            Tree.Node IMyVirtualMass_Nodes = new Tree.Node(MenuNodes); IMyVirtualMass_Nodes.caption = "VirtualMass";
            Tree.Node IMyWarhead_Nodes = new Tree.Node(MenuNodes); IMyWarhead_Nodes.caption = "Warhead";

            addBlocks<IMyAirVent>(IMyAirVent_Nodes);
            addBlocks<IMyAssembler>(IMyAssembler_Nodes);
            addBlocks<IMyBatteryBlock>(IMyBatteryBlock_Nodes);
            addBlocks<IMyBeacon>(IMyBeacon_Nodes);
            addBlocks<IMyButtonPanel>(IMyButtonPanel_Nodes);
            addBlocks<IMyCameraBlock>(IMyCameraBlock_Nodes);
            addBlocks<IMyCargoContainer>(IMyCargoContainer_Nodes);
            addBlocks<IMyCockpit>(IMyCockpit_Nodes);
            addBlocks<IMyCollector>(IMyCollector_Nodes);
            addBlocks<IMyConveyorSorter>(IMyConveyorSorter_Nodes);
            addBlocks<IMyDoor>(IMyDoor_Nodes);
            addBlocks<IMyGravityGenerator>(IMyGravityGenerator_Nodes);
            addBlocks<IMyGyro>(IMyGyro_Nodes);
            addBlocks<IMyInteriorLight>(IMyInteriorLight_Nodes);
            addBlocks<IMyJumpDrive>(IMyJumpDrive_Nodes);
            addBlocks<IMyLandingGear>(IMyLandingGear_Nodes);
            addBlocks<IMyLargeGatlingTurret>(IMyLargeGatlingTurret_Nodes);
            addBlocks<IMyLargeInteriorTurret>(IMyLargeInteriorTurret_Nodes);
            addBlocks<IMyLargeMissileTurret>(IMyLargeMissileTurret_Nodes);
            addBlocks<IMyLargeTurretBase>(IMyLargeTurretBase_Nodes);
            addBlocks<IMyLaserAntenna>(IMyLaserAntenna_Nodes);
            addBlocks<IMyLightingBlock>(IMyLightingBlock_Nodes);
            addBlocks<IMyMedicalRoom>(IMyMedicalRoom_Nodes);
            addBlocks<IMyMotorAdvancedStator>(IMyMotorAdvancedStator_Nodes);
            addBlocks<IMyMotorBase>(IMyMotorBase_Nodes);
            addBlocks<IMyMotorRotor>(IMyMotorRotor_Nodes);
            addBlocks<IMyMotorStator>(IMyMotorStator_Nodes);
            addBlocks<IMyMotorSuspension>(IMyMotorSuspension_Nodes);
            addBlocks<IMyOreDetector>(IMyOreDetector_Nodes);
            addBlocks<IMyOxygenFarm>(IMyOxygenFarm_Nodes);
            addBlocks<IMyOxygenGenerator>(IMyOxygenGenerator_Nodes);
            addBlocks<IMyOxygenTank>(IMyOxygenTank_Nodes);
            addBlocks<IMyPistonBase>(IMyPistonBase_Nodes);
            addBlocks<IMyPistonTop>(IMyPistonTop_Nodes);
            addBlocks<IMyProductionBlock>(IMyProductionBlock_Nodes);
            addBlocks<IMyProgrammableBlock>(IMyProgrammableBlock_Nodes);
            addBlocks<IMyProjector>(IMyProjector_Nodes);
            addBlocks<IMyRadioAntenna>(IMyRadioAntenna_Nodes);
            addBlocks<IMyReactor>(IMyReactor_Nodes);
            addBlocks<IMyRefinery>(IMyRefinery_Nodes);
            addBlocks<IMyReflectorLight>(IMyReflectorLight_Nodes);
            addBlocks<IMyRemoteControl>(IMyRemoteControl_Nodes);
            addBlocks<IMySensorBlock>(IMySensorBlock_Nodes);
            addBlocks<IMyShipConnector>(IMyShipConnector_Nodes);
            addBlocks<IMyShipController>(IMyShipController_Nodes);
            addBlocks<IMyShipDrill>(IMyShipDrill_Nodes);
            addBlocks<IMyShipGrinder>(IMyShipGrinder_Nodes);
            addBlocks<IMyShipMergeBlock>(IMyShipMergeBlock_Nodes);
            addBlocks<IMyShipToolBase>(IMyShipToolBase_Nodes);
            addBlocks<IMyShipWelder>(IMyShipWelder_Nodes);
            addBlocks<IMySlimBlock>(IMySlimBlock_Nodes);
            addBlocks<IMySmallGatlingGun>(IMySmallGatlingGun_Nodes);
            addBlocks<IMySmallMissileLauncher>(IMySmallMissileLauncher_Nodes);
            addBlocks<IMySmallMissileLauncherReload>(IMySmallMissileLauncherReload_Nodes);
            addBlocks<IMySolarPanel>(IMySolarPanel_Nodes);
            addBlocks<IMySoundBlock>(IMySoundBlock_Nodes);
            addBlocks<IMySpaceBall>(IMySpaceBall_Nodes);
            addBlocks<IMyTextPanel>(IMyTextPanel_Nodes);
            addBlocks<IMyThrust>(IMyThrust_Nodes);
            addBlocks<IMyTimerBlock>(IMyTimerBlock_Nodes);
            addBlocks<IMyUserControllableGun>(IMyUserControllableGun_Nodes);
            addBlocks<IMyVirtualMass>(IMyVirtualMass_Nodes);
            addBlocks<IMyWarhead>(IMyWarhead_Nodes);

            StringBuilder Content = new StringBuilder();
            View(MenuNodes, ref Content);

            Echo(Content.ToString());
        }

        private void View(Tree.Node Node, ref StringBuilder sb)
        {
            List<Tree.Node> Childs = Node.getChilds();
            for(int i = 0; i < Childs.Count; i++)
            {
                Tree.Node Child = Childs[i];
                sb.AppendLine((new String('-', Child.getDepth())) + Child.getCaption());
                View(Child, ref sb);                
            }
        }

        private void addBlocks<T>(Tree.Node ParentNode)
        {
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, (x => (x is T)));
            for(int i = 0; i < Blocks.Count; i++)
            {
                Tree.BlockNode BlockNode = new Tree.BlockNode(ParentNode);
                BlockNode.Block = Blocks[i];
                addActions(BlockNode);
            }
        }

        private void addActions(Tree.BlockNode Parent)
        {
            if(Parent.Block != null)
            {
                List<ITerminalAction> Actions = new List<ITerminalAction>();
                Parent.Block.GetActions(Actions);
                for(int i = 0; i < Actions.Count; i++)
                {
                    Tree.ActionNode ActionNode = new Tree.ActionNode(Parent);
                    ActionNode.Action = Actions[i];
                }
            }
        }


        class Tree
        {
            public class ActionNode : Node
            {
                public ITerminalAction Action;

                public void ApplyAction()
                {
                    if (Action != null && (getParent() is BlockNode))
                    {
                        (getParent() as BlockNode).ApplyAction(Action.Id);
                    }
                }

                public ActionNode(Node Parent = null)
                {
                    setParent(Parent);
                }

                public override string getCaption()
                {
                    if (Action != null)
                    {
                        if ((getParent() is BlockNode) && (getParent() as BlockNode).Block != null)
                        {
                            return Action.Id + " @ " +(getParent() as BlockNode).Block.CustomName;
                        }
                        else
                        {
                            return Action.Id;
                        }
                    }
                    else
                    {
                        return base.getCaption();
                    }
                }
            }

            public class BlockNode : Node
            {
                public IMyTerminalBlock Block;

                public void ApplyAction(string action)
                {
                    if (Block != null && Block.HasAction(action))
                    {
                        Block.ApplyAction(action);
                    }
                }

                public BlockNode(Node Parent = null)
                {
                    setParent(Parent);
                }

                public override string getCaption()
                {
                    if(Block != null)
                    {
                        return Block.CustomName;
                    } else
                    {
                        return base.getCaption();
                    }                    
                }
            }

            public class Node
            {
                private List<Node> Childs = new List<Node>();
                private Node Parent = null;
                public string caption;

                public virtual string getCaption()
                {
                    return caption;
                }

                public Node(Node Parent = null)
                {
                    setParent(Parent);
                }

                public void setParent(Node Parent)
                {
                    if (!Parent.Equals(this))
                    {
                        if (hasParent())
                        {
                            getParent().Childs.Remove(this);
                        }
                        this.Parent = Parent;
                        if (hasParent())
                        {
                            getParent().Childs.Add(this);
                        }
                    }
                }

                public Node getParent()
                {
                    return Parent;
                }

                public bool hasParent()
                {
                    return (Parent != null);
                }

                public void addChild(Node Child)
                {
                    Child.setParent(this);
                }

                public void removeChild(Node Child)
                {
                    if (hasChild(Child))
                    {
                        Child.setParent(null);
                    }                    
                }

                public bool hasChild(Node Child)
                {
                    return Childs.Contains(Child);
                }

                public Node getRoot()
                {
                    if (hasParent())
                    {
                        return getParent().getRoot();
                    }
                    else
                    {
                        return this;
                    }
                }

                public int getDepth()
                {
                    if (hasParent())
                    {
                        return getParent().getDepth() + 1;
                    } else
                    {
                        return 0;
                    }
                }

                public List<Node> getChilds()
                {
                    return Childs;
                }
                
            }
        }

        #endregion End of  Game Code - Copy/Paste Code from this region into Block Script Window in Game
    }
}