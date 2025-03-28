﻿---
--- Generated by EmmyLua(https://github.com/EmmyLua)
--- Created by Z_End.
--- DateTime: 2025/3/11 23:34
---

CS = import('Assembly-CSharp', 'global::')
CS.UE = import('UnityEngine')

StatModifier = {}

function StatModifier.new(idOrName, valMod, rateMod)
    valMod = valMod or 0
    rateMod = rateMod or 0
    local stat = Game:GetItem(idOrName)
    ---@class StatModifier
    local mod = CS.StatModifier()
    mod.Stat = stat.UniqueIDScriptable
    mod.ValueModifier = CS.UE.Vector2(valMod, valMod)
    mod.RateModifier = CS.UE.Vector2(rateMod, rateMod)
    return mod
end

---@class CountedCard
---@field Card UniqueIdObjectBridge
---@field Count number
CountedCard = {}

---@param idOrName string
---@param count number
function CountedCard.new(idOrName, count)
    local ins = { Card = Game:GetItem(idOrName), Count = count or 1 }
    return ins
end

---@class BpStageEle:CountedCard
---@field Spend boolean
---@field UsageCost number
BpStageEle = {}

---@param spendUsage boolean|nil
function BpStageEle.new(idOrName, cost, spendUsage)
    ---@type BpStageEle
    local ins = CountedCard.new(idOrName, (spendUsage and 1) or cost)
    ins.Spend = not spendUsage
    ins.UsageCost = cost
    return ins
end

CardUtil = {}

---@param card UniqueIdObjectBridge
---@param multi number|nil
---@param extraMods StatModifier[]|nil
function CardUtil.SetCommonBpStatMods(card, multi, extraMods)
    local modList = extraMods or {}
    multi = multi or 1
    local fac = multi * card.CardData.BuildingDaytimeCost
    table.insert(modList, StatModifier.new("耐力", -1.25 * fac))
    table.insert(modList, StatModifier.new("幸福度", 0.3 * fac))
    table.insert(modList, StatModifier.new("专注度", 0.2 * fac))

    card:SetBuildStatCost(modList)
end

---@param card UniqueIdObjectBridge|InGameCardBridge
function CardUtil.FindFirstCard(card, includeBackground)
    local find = CardUtil.FindCards(card, includeBackground)
    if #find > 0 then
        return find[1]
    end
end

---@param tag string
function CardUtil.FindFirstCardByTag(tag, includeBackground)
    local find = CardUtil.FindCardsByTag(tag, includeBackground)
    if #find > 0 then
        return find[1]
    end
end

---@param card UniqueIdObjectBridge|InGameCardBridge
function CardUtil.FindCards(card, includeBackground)
    includeBackground = includeBackground or false
    ---@type UniqueIdObjectBridge
    local cardData = card.CardModel or card
    ---@type InGameCardBridge[]
    local cache = {}
    Game:FindCards(cardData, cache, includeBackground)
    return cache
end

---@param tag string
function CardUtil.FindCardsByTag(tag, includeBackground)
    includeBackground = includeBackground or false
    ---@type InGameCardBridge[]
    local cache = {}
    Game:FindCardsByTag(tag, cache, includeBackground)
    return cache
end
