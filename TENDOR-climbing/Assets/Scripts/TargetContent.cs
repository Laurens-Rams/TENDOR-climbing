using UnityEngine;

public abstract class TargetContent : MonoBehaviour
{
  public abstract void Show();
  public abstract void Hide();
  public abstract void Pause();
  public abstract void Unpause();

  public virtual void Tick() { }
}
