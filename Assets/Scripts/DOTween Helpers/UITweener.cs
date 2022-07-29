using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UITweener : MonoBehaviour
{
    public UISequenceManager sequenceManager;

    [System.Serializable]
    public enum TweenType {
        Entry = 0,
        Exit = 1
    }

    [System.Serializable]
    public class UITween
    {
        public TweenType tweenType;
        public UIAnimation UIAnimation;
        public GameObject objectToAnimate;

        public GameEvent onComplete;
        public GameEvent onStart;
    }

    public List<UITween> UITweens;
    public Dictionary<TweenType,UITween> UITweensDict;

    public void AppendTweener(int i)
    {
        _AppendTweener((TweenType)i);
    }

    public void AppendTweener(TweenType tweenType)
    {
        _AppendTweener(tweenType);
    }

    private void _AppendTweener(TweenType tweenType)
    {
        UITween UITween = UITweensDict[tweenType];
        Tween _tween = CreateTweener(UITween);

        if (_tween != null)
        {
            if (sequenceManager.currentSequence == null)
                CreateSequence();

            sequenceManager.currentSequence.Append(_tween);
        }
            
    }

    public void JoinTweener(int i)
    {
        _JoinTweener((TweenType)i);
    }

    public void JoinTweener(TweenType tweenType)
    {
        _JoinTweener(tweenType);
    }

    private void _JoinTweener(TweenType tweenType)
    {
        UITween UITween = UITweensDict[tweenType];
        Tween _tween = CreateTweener(UITween);

        if (_tween != null)
        {
            if (sequenceManager.currentSequence == null)
                CreateSequence();

            sequenceManager.currentSequence.Join(_tween);
        }
    }

    private void CreateSequence()
    {
        sequenceManager.currentSequence = DOTween.Sequence();
        sequenceManager.currentSequence.OnKill( () => {
            sequenceManager.currentSequence = null;
        });
    }

    private void Awake()
    {
        UITweensDict = new Dictionary<TweenType, UITween>();
        foreach (UITween UITween in UITweens)
        {
            if (UITween.objectToAnimate == null)
                UITween.objectToAnimate = gameObject;

            UITweensDict[UITween.tweenType] = UITween;
        }
    }

    private Tween CreateTweener(UITween UITween)
    {
        switch (UITween.UIAnimation.animationType)
        {
            case (UIAnimation.AnimationType.Fade):
                return Fade(UITween);
        }

        return null;
    }

    private Tween Fade(UITween UITween)
    {
        if (UITween.objectToAnimate.GetComponent<CanvasGroup>() == null)
            UITween.objectToAnimate.AddComponent<CanvasGroup>();

        if (UITween.objectToAnimate.GetComponent<Canvas>() == null)
            UITween.objectToAnimate.AddComponent<Canvas>();

        CanvasGroup canvasGroup = UITween.objectToAnimate.GetComponent<CanvasGroup>();
        Canvas canvas = UITween.objectToAnimate.GetComponent<Canvas>();

        UIAnimation UIAnimation = UITween.UIAnimation;

        Tween _tween = canvasGroup.DOFade(UIAnimation.to.x, UIAnimation.duration);

        _tween.OnStart( () =>
        {

            if (!canvas.enabled && UIAnimation.to.x != 0)
                canvas.enabled = true;

            if (UIAnimation.useFrom)
                canvasGroup.alpha = UIAnimation.from.x;

            if (UITween.onStart != null)
                UITween.onStart.Raise();
                
        } );

        _tween.OnComplete(() =>
        {
            if (UIAnimation.to.x == 0)
                canvas.enabled = false;

            if (UITween.onComplete != null)
                UITween.onComplete.Raise();

        });

        return _tween;
    }
}
