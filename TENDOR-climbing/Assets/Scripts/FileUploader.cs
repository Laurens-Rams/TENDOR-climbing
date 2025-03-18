using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class FileUploader : MonoBehaviour
{
    public void StartUpload(byte[] data)
    {
        StartCoroutine(Upload(data));
    }

    IEnumerator Upload(byte[] data)
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        //formData.Add(new MultipartFormDataSection("file=foo&field2=bar"));
        formData.Add(new MultipartFormFileSection(data));

        UnityWebRequest www = UnityWebRequest.Post("https://file.io", formData);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
            Debug.Log(www.error);
        else
            Debug.Log(www.downloadHandler.text);
    }
}
