using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Realtime;
using Photon.Pun;
using ExitGames.Client.Photon;

public class Emoji_Chat : MonoBehaviourPunCallbacks, IOnEventCallback
{
    enum EmojiType
    {
        Emoji = 0,
        Word = 1
    }

    [SerializeField] private GameObject emojiPanel;
    [SerializeField] private GameObject[] emoji;
    [SerializeField] private GameObject[] word;


    void Start()
    {
        
    }
    
    void Update()
    {
        
    }

    public void SendEmoji()
    {
        int index1 = EventSystem.current.currentSelectedGameObject.GetComponent<EmojiInfo>().emojiType;
        int index2 = EventSystem.current.currentSelectedGameObject.GetComponent<EmojiInfo>().emojiIndex;

        SyncEmojiIndex(new object[] { index1, index2 });
    }

    public void EmojiBtnClick()
    {
        if (emojiPanel.activeSelf)
        {
            emojiPanel.SetActive(false);
        }
        else
        {
            emojiPanel.SetActive(true);
        }
    }

    IEnumerator DelayToShowEmoji(int index1, int index2)
    {
        for(int i = 0; i < emoji.Length; i++) emoji[i].SetActive(false);

        for(int i = 0; i < word.Length; i++) word[i].SetActive(false);
        
        if(index1 == (int)EmojiType.Emoji)
        {
            emoji[index2].SetActive(true);
        }
        else
        {
            word[index2].SetActive(true);
        }
        
        yield return new WaitForSeconds(3f);

        for (int i = 0; i < emoji.Length; i++) emoji[i].SetActive(false);

        for (int i = 0; i < word.Length; i++) word[i].SetActive(false);
    }

    #region Photon Event

    public void SyncEmojiIndex(object[] content)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(105, content, raiseEventOptions, SendOptions.SendReliable);
    }
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if(eventCode == 105)
        {
            object[] infos = (object[])photonEvent.CustomData;

            StopAllCoroutines();
            StartCoroutine(DelayToShowEmoji((int)infos[0], (int)infos[1]));
        }
    }

    #endregion
}
