using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(UITweener))]
public class GenericWindow1 : MonoBehaviour
{
    public UITweener uiTweener;

    public bool isOpen = true;

    private void Awake()
    {
        uiTweener = GetComponent<UITweener>();
    }

    public virtual void Open()
    {

        if (!isOpen)
        {
            uiTweener.JoinTweener(UITweener.TweenType.Entry);
            isOpen = true;
        }
    }

    public virtual void Open(bool doAppend)
    {
        if (!isOpen)
        {
            //Debug.Log(uiTweener.sequenceManager.name);
            //if (uiTweener.sequenceManager.currentSequence != null)
            //{
            //    Debug.Log(uiTweener.sequenceManager.currentSequence.Duration());
            //}
            //else
            //{
            //    Debug.Log("current sequence is null");
            //}
            

            if (doAppend)
                uiTweener.AppendTweener(UITweener.TweenType.Entry);
            else
                uiTweener.JoinTweener(UITweener.TweenType.Entry);

            isOpen = true;

        }


    }

    public virtual void Open(float atPosition)
    {
        if (!isOpen)
        {
            uiTweener.InsertTweener(atPosition, UITweener.TweenType.Entry);
            isOpen = true;
        }
    }

    public void OpenAndCompleteAsync()
    {
        Tween _tween = uiTweener.GetTween(UITweener.TweenType.Entry);
        _tween.Complete();
        isOpen = true;
    }

    public Tween GetOpen()
    {
        Sequence entrySequence = DOTween.Sequence();

        Tween entry = uiTweener.GetTween(UITweener.TweenType.Entry);
        entrySequence.Join(entry);

        entrySequence.OnStart(() => { isOpen = true; });

        return entrySequence;
    }

    public virtual void Close()
    {
        if (isOpen && uiTweener != null)
        {
            //Debug.Log("Closing " + gameObject.name);
            uiTweener.JoinTweener(UITweener.TweenType.Exit);
            isOpen = false;
        }
    }

    public virtual void Close(bool doAppend)
    {
        if (isOpen)
        {
            if (doAppend)
                uiTweener.AppendTweener(UITweener.TweenType.Exit);
            else
                uiTweener.JoinTweener(UITweener.TweenType.Exit);

            isOpen = false;
        }
    }

    public virtual void Close(float atPosition)
    {
        if (isOpen)
        {
            uiTweener.InsertTweener(atPosition, UITweener.TweenType.Exit);
            isOpen = false;
        }
    }

    public void CloseAndCompleteAsync()
    {
        if (isOpen)
        {
            Tween _tween = uiTweener.GetTween(UITweener.TweenType.Exit);
            _tween.Complete();
            isOpen = false;
        }
    }

    public Tween GetClose()
    {
        Sequence exitSequence = DOTween.Sequence();

        Tween entry = uiTweener.GetTween(UITweener.TweenType.Exit);
        exitSequence.Join(entry);

        exitSequence.OnComplete(() => { isOpen = false; });

        return exitSequence;
    }

    public virtual void Pulse()
    {
        Sequence pulse = GetPulse();

        if (uiTweener.sequenceManager.currentSequence == null)
            uiTweener.sequenceManager.currentSequence = DOTween.Sequence();

        uiTweener.sequenceManager.currentSequence.Join(pulse);
    }

    public virtual void Pulse(float atPosition)
    {
        Sequence pulse = GetPulse();

        if (uiTweener.sequenceManager.currentSequence == null)
            uiTweener.sequenceManager.currentSequence = DOTween.Sequence();

        uiTweener.sequenceManager.currentSequence.Insert(atPosition, pulse);
    }

    public virtual void PulseIfOpen()
    {
        if (isOpen)
        {
            Pulse();
        }
    }

    public virtual void PulseIfOpen(GenericWindow1 target)
    {
        if (target.isOpen)
            Pulse();
    }

    public Sequence GetPulse()
    {
        Sequence pulse = DOTween.Sequence();

        Tween entry = uiTweener.GetTween(UITweener.TweenType.Entry);
        Tween exit = uiTweener.GetTween(UITweener.TweenType.Exit);
        pulse.Join(entry);
        pulse.Insert(entry.Duration(), exit);

        return pulse;
    }

    //public virtual void Pulse(bool doAppend)
    //{
    //    if (doAppend)
    //        uiTweener.AppendTweener(UITweener.TweenType.Entry);
    //    else
    //        uiTweener.JoinTweener(UITweener.TweenType.Entry);

    //    uiTweener.AppendTweener(UITweener.TweenType.Exit);
    //}
}
