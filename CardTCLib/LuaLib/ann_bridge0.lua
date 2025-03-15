﻿---
--- Created by Z_End.
--- DateTime: 2025/3/7 23:19
---

---@alias bpMainTab "生存"|"工具"|"狩猎"|"纺织"|"冶金 & 制陶"|"农业"|"魔法"|string
---@alias bpSubTab "火"|"物资"|"医疗"|"娱乐"|"工具"|"高级工具"|"材料"|"家具"|"建造房屋"|"钓鱼"|"陷阱"|"近战"|"简易的"|"布"|"皮革"|"毛皮"|"装备"|"工具"|"器具"|"农业"|"动物养殖"|"圣地"|"灵具"|string
---@alias enumDurability "Usage"|"Fuel"|"Progress"|"Spoilage"|"Special1"|"Special2"|"Special3"|"Special4"

---@class UniqueIdObjectBridge
---@field CardData CardData
---
---@field GetTValue fun(self:UniqueIdObjectBridge,key:string):number
---@field SetTValue fun(self:UniqueIdObjectBridge,key:string,val:number)
---
---@field SetBpResults fun(self:UniqueIdObjectBridge,data:CountedCard[])
---@field SetBpStage fun(self:UniqueIdObjectBridge,idx:number,data:BpStageEle[])
---@field SetBpTab fun(self:UniqueIdObjectBridge,mainTab:bpMainTab,subTab:bpSubTab)
---@field InitEmptyBpRequire fun(self:UniqueIdObjectBridge)
---@field SetBuildStatCost fun(self:UniqueIdObjectBridge,mods:StatModifier[])
---
---@field GenNpcCardModel fun(self:UniqueIdObjectBridge):UniqueIdObjectBridge
---@field AddAction fun(self:UniqueIdObjectBridge,id:string,name:string,type:CardActionType)
---@field AddAction fun(self:UniqueIdObjectBridge,action:CardActionBridge)
---@field RemoveAction fun(self:UniqueIdObjectBridge,type:CardActionType,id:string)
---@field GetStatInfo fun(self:UniqueIdObjectBridge):number,number,number,number
---@field SetStat fun(self:UniqueIdObjectBridge,newValue:number,newRate:number)
---@field SetStatEnum fun(self:UniqueIdObjectBridge,newValue:number,newRate:number):IEnumerator
---@field AddStat fun(self:UniqueIdObjectBridge,newValue:number,newRate:number)
---@field AddStatEnum fun(self:UniqueIdObjectBridge,newValue:number,newRate:number):IEnumerator
---@field Name string
---@field NameLocal string
---@field NameChinese string
---@field SetIcon fun(self:UniqueIdObjectBridge,icon:string)
---@field GetItem fun(self:UniqueIdObjectBridge,key:string):number
---@field SetItem fun(self:UniqueIdObjectBridge,key:string):number
---@field Desc string
---@field Weight number
---@field MaxWeightCapacity number
---@field AddSlot fun(self:UniqueIdObjectBridge,add:UniqueIdObjectBridge|nil)
---@field RemoveSlot fun(self:UniqueIdObjectBridge,idx:number)
---
---@field SetDurabilityStat fun(self:UniqueIdObjectBridge,type:enumDurability,stat:DurabilityStat)
---@field GetDurabilityStat fun(self:UniqueIdObjectBridge,type:enumDurability):DurabilityStat

---@class InGameCardBridge
---@field CardModel UniqueIdObjectBridge
---@field SetExtraValue fun(self:InGameCardBridge,key:string,val:string|number)
---@field GetExtraValue fun(self:InGameCardBridge,key:string):string|number|nil
---@field AddCard fun(self:InGameCardBridge,obj:UniqueIdObjectBridge,arg:AddCardExtraArg|nil)
---@field AddCardEnum fun(self:InGameCardBridge,obj:UniqueIdObjectBridge,arg:AddCardExtraArg|nil):IEnumerator
---@field Delete fun(self:InGameCardBridge)
---@field DeleteEnum fun(self:InGameCardBridge):IEnumerator
---@field ResetInventory fun(self:InGameCardBridge)
---@field ResetInventoryEnum fun(self:InGameCardBridge):IEnumerator
---@field ClearInventory fun(self:InGameCardBridge)
---@field ClearInventoryEnum fun(self:InGameCardBridge):IEnumerator
---@field TransformTo fun(self:InGameCardBridge,target:UniqueIdObjectBridge)
---@field TransformToEnum fun(self:InGameCardBridge,target:UniqueIdObjectBridge):IEnumerator
---@field InventoryCardsCount number
---@field HasTag fun(self:InGameCardBridge,tag:string):boolean
---@field GetItem fun(self:InGameCardBridge,idx:number):InGameCardBridge[]
---@field GetDurability fun(self:InGameCardBridge,key:enumDurability):number
---@field SetDurability fun(self:InGameCardBridge,key:enumDurability,val:number)
---@field SetDurabilityEnum fun(self:InGameCardBridge,key:enumDurability,val:number):IEnumerator
---@field AddDurability fun(self:InGameCardBridge,key:string,val:number)
---@field AddDurabilityEnum fun(self:InGameCardBridge,key:string,val:number):IEnumerator
