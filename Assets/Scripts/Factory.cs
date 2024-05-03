using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Factory : MonoBehaviour
{
    [Header("Base Options")]
    public float baseAmount;
    public bool buyable = true;
    public bool instant;

    [Header("Custom Decription (Leave blank for Default)")]
    [TextArea(3, 3)]
    public string customDesc;

    [Header("Inputs:")]
    public List<FacObj> inputs = new List<FacObj>();

    [Header("Outputs:")]
    public List<FacObj> outputs = new List<FacObj>();

    [Header("Cap Settings")]
    public bool capped;
    public float baseCap;
    public float baseCapMult;
    public bool soft = true;
    private float produced;

    [Header("Timer Settings")]
    public bool timer;
    public bool countDown = true;
    public bool timerCost;
    public string timerName;
    public float baseTimerCap;

    [HideInInspector]
    public float cap, capMult, amount, timerAmount, timerCap;

    [Header("Button Hooks")]
    public TMP_Text bText;
    public TMP_Text mText;
    public RawImage mask;

    private void Start()
    {
        Restart();
    }

    private void Update()
    {
        OnUpdate();

        if (!instant) Produce(Time.deltaTime);
        if (timer) UpdateTimer();

        UpdateOutputs();
        if (bText && bText.IsActive()) bText.text = UpdateAmount();
        if (mText && mText.IsActive()) UpdatePrices();
    }

    public virtual void OnPointerEnter()
    {
        mask.gameObject.SetActive(true);
    }

    public virtual void OnPointerExit()
    {
        mask.gameObject.SetActive(false);
    }

    public virtual void Add(float num, int type = 0, bool mult = false, int target = 0)
    {
        switch (type)
        {
            case 0:
                OnAdd(num, mult);
                break;
            case 1:
                UpgradeCap(num, mult);
                break;
            case 2:
                UpgradeMult(num, mult, target);
                break;
            case 3:
                UpgradeTimer(num, mult);
                break;
            case 4:
                UpgradeTimerCap(num, mult);
                break;
        }
    }

    public virtual void OnUpdate()
    {

    }

    public virtual void Produce(float time = 1)
    {
        foreach (var item in outputs)
        {
            item.fac.Add(item.value * time, (int)item.type, item.isMult, item.target);
        }
    }

    public virtual string UpdateAmount()
    {
        var text = gameObject.name + "\n(";
        if (instant && buyable && !timer && (amount == 0)) return text + "Click)";

        text += Round(amount);
        if (capped) text += "/" + Round(cap);
        text += ")";

        if (timer) text += string.Format("\n{0}: ({1}/{2})", timerName, Round(timerAmount), Round(timerCap));

        return text;
    }

    public virtual string Suffix(FacObj item)
    {
        switch ((int)item.type)
        {
            case 1:
                return "Cap";
            case 2:
                if (item.target > 0)
                {
                    var name = item.fac.outputs[item.target - 1].fac.gameObject.name;
                    return name + " Mult";
                }
                return "Mult";
            case 3:
                return item.fac.timerName;
            case 4:
                return item.fac.timerName + " Cap";
            default:
                return "";
        }
    }

    public virtual string InString(FacObj item)
    {
        var text = Round(item.value, -2) + " " + item.fac.gameObject.name;
        return text + " " + Suffix(item);
    }

    public virtual string OutString(FacObj item)
    {
        var text = Round(item.value, -2) + " " + item.fac.gameObject.name;
        return text + " " + Suffix(item) + (instant ? "" : "/s");
    }

    public virtual string OutTitle()
    {
        if (timer) return "Produce When \n" + timerName + " Reaches " + (countDown ? "0:" : "Max:");
        if (instant) return "Produce On Activate:";
        return "Producing:";
    }

    public virtual string InTitle()
    {
        if(timer) return "Cost When \n" + timerName + " Reaches " + (countDown ? "0:" : "Max:");
        if (instant) return "Cost On Activate:";
        return "Cost For Next:";
    }

    public virtual string MenuString()
    {
        if (customDesc != "") return customDesc;

        string outText = "";
        foreach (var item in outputs)
        {
            outText += (item.hide ? "" : "\n " + (item.isMult ? "x" : "" ) + OutString(item));
        }

        string inText = "";
        foreach (var item in inputs)
        {
            inText += (item.hide ? "" : "\n " + (item.isMult ? "x" : "") + InString(item));
        }

        if (inText == "")
        {
            if(outText == "")
            {
                return "This is " + gameObject.name;
            }
            return OutTitle() + outText;
        }

        if (outText == "") return InTitle() + inText;


        return OutTitle() + outText + "\n\n" + InTitle() + inText;
    }

    public void UpdatePrices()
    {
        mText.SetText(MenuString());
        mask.rectTransform.sizeDelta = mText.GetPreferredValues();
    }

    public void UpdateTimer()
    {
        timerAmount = Mathf.Clamp(timerAmount, 0, timerCap);
        if((countDown && (timerAmount == 0)) || (!countDown && (timerAmount == timerCap)))
        {
            if(timerCost)
            {
                if(CheckActive())
                {
                    OnActivate();
                    RestartTimer();
                } 
            }
            else
            {
                Sell();
                RestartTimer();
            }
        }
    }

    public void Click()
    {
        if (!buyable) return;
        buyable = false;

        if (CheckActive()) OnActivate();

        buyable = true;
    }

    public virtual bool CheckActive()
    {
        if(capped)
        {
            if (soft && amount >= cap) return false;
            if (!soft && produced >= cap) return false;
        }

        foreach (var item in inputs)
        {
            if (item.fac.amount < item.value) return false;
        }

        return true;
    }

    public void OnActivate()
    {
        Buy();
        Sell();
    }

    public virtual void Buy()
    {
        foreach (var item in inputs)
        {
            var sign = item.isMult ? 1 : -1;
            item.fac.Add(sign * item.value, (int)item.type, item.isMult, item.target);
            item.value *= item.isMult ? 1 : item.mult;
        }
    }

    public virtual void Sell()
    {
        if(instant)
        {
            Produce(1);
        }
        else
        {
            Add(1);
        }
    }

    public virtual void UpdateOutputs()
    {
        float mult = instant ? 1 : amount;
        foreach (var item in outputs)
        {
            item.value = item.baseValue * item.mult * mult;
        }
    }

    public string Round(float num, int pow = 0)
    {
        var power = Mathf.Pow(10, pow);
        var round = power * Mathf.Round(num / power);
        return round.ToString();
    }

    public virtual void Restart()
    {
        cap = baseCap;
        capMult = baseCapMult;
        produced = 0;

        timerCap = baseTimerCap;
        RestartTimer();

        amount = baseAmount;

        foreach (var item in inputs)
        {
            item.Init();
        }

        foreach (var item in outputs)
        {
            item.Init();
        }

        UpdateOutputs();
    }

    public virtual void RestartTimer()
    {
        timerAmount = 0;
        if (countDown) timerAmount = timerCap;
    }

    public virtual void OnAdd(float num, bool isMult)
    {
        if (isMult) num = (num - 1) * amount;

        if (capped)
        {
            if (soft)
            {
                amount = Mathf.Min(amount + num, cap);
            }
            else
            {
                if (produced >= cap) return;

                produced += num;
                amount += num - Mathf.Max(produced - cap, 0);
            }
        }
        else
        {
            amount = Mathf.Max(amount + num, 0);
        }
    }

    public virtual void UpgradeCap(float num, bool isMult)
    {
        if (isMult) num = (num - 1) * cap;
        cap = Mathf.Max(cap + num, 0);
    }

    public virtual void UpgradeMult(float num, bool isMult, int target)
    {
        if (target < 0 || target > outputs.Count) return;

        if(target == 0)
        {
            foreach (var item in outputs)
            {
                var itemNum = isMult ? item.mult * num : item.mult + num;
                item.mult = Mathf.Max(itemNum, 0);
            }
        } 
        else
        {
            var itemNum = isMult ? outputs[target - 1].mult * num : outputs[target - 1].mult + num;
            outputs[target - 1].mult = Mathf.Max(itemNum, 0);
        }
        
    }

    public virtual void UpgradeTimer(float num, bool isMult)
    {
        if (isMult) num = (num - 1) * timerAmount;
        timerAmount += num;
    }

    public virtual void UpgradeTimerCap(float num, bool isMult)
    {
        if (isMult) num = (num - 1) * timerCap;
        timerCap = Mathf.Max(timerCap + num, 0);
    }

}
