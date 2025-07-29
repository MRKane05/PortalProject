using UnityEngine;
using UnityEngine.Events;

//This calls an event when the player walks into the volume
public class MusicTriggerVolume : TriggerVolume {
    public AudioClip musicToPlay;
    public bool bLooping = false;
    public override void DoTrigger()
    {
        base.DoTrigger();
        if (musicToPlay && MusicManager.Instance)
        {
            MusicManager.Instance.playMusic(musicToPlay, bLooping);
        }
    }
}
