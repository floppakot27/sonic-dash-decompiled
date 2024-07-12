using System;
using System.Collections;
using UnityEngine;

public class BundleGui : MonoBehaviour
{
	[SerializeField]
	private GameObject m_waitingForConnectionGroup;

	[SerializeField]
	private GameObject m_downloadingGroup;

	[SerializeField]
	private UILabel m_waitingForConnectionText;

	[SerializeField]
	private UILabel m_bundleDownloadText;

	[SerializeField]
	private UILabel m_bundleCountText;

	[SerializeField]
	private UISlider m_bundleDownloadProgress;

	private bool m_delayedWarningShown;

	private readonly string[] WaitingForConnectionText = new string[11]
	{
		"WAITING FOR INTERNET CONNECTION", "WAITING FOR INTERNET CONNECTION", "EN ATTENTE DE CONNEXION INTERNET", "IN ATTESA DELLA CONNESSIONE A INTERNET", "WARTEN AUF INTERNET-VERBINDUNG", "ESPERANDO UNA CONEXIÓN A INTERNET", "AGUARDANDO CONEXÃO COM A INTERNET", "В ПРОЦЕССЕ ПОДКЛЮЧЕНИЯ К ИНТЕРНЕТУ", "WAITING FOR INTERNET CONNECTION", "WAITING FOR INTERNET CONNECTION",
		"WAITING FOR INTERNET CONNECTION"
	};

	private readonly string[] DownloadingContentText = new string[11]
	{
		"DOWNLOADING ADDITIONAL CONTENT", "DOWNLOADING ADDITIONAL CONTENT", "TÉLÉCHARGEMENT DU CONTENU SUPPLÉMENTAIRE", "SCARICAMENTO DEL CONTENUTO AGGIUNTIVO", "ZUSäTZLICHE INHALTE WERDEN HERUNTERGELADEN", "DESCARGANDO CONTENIDO ADICIONAL", "BAIXANDO CONTEÚDO ADICIONAL", "ЗАГРУЖАЕТСЯ ДОПОЛНИТЕЛЬНОЕ СОДЕРЖАНИЕ", "DOWNLOADING ADDITIONAL CONTENT", "DOWNLOADING ADDITIONAL CONTENT",
		"DOWNLOADING ADDITIONAL CONTENT"
	};

	private readonly string[] RemainingItemsText = new string[11]
	{
		"{0} OF {1}", "{0} OF {1}", "{0} DE {1}", "{0} DI {1}", "{0} VON {1}", "{0} DE {1}", "{0} DE {1}", "{0} ИЗ {1}", "{0} OF {1}", "{0} OF {1}",
		"{0} OF {1}"
	};

	private readonly string[] OSWarningBodyText = new string[11]
	{
		"Occasionally Sonic Dash needs to connect to the Internet to download additional content.\n\nPlease check your internet connection and try again.", "Occasionally Sonic Dash needs to connect to the Internet to download additional content.\n\nPlease check your internet connection and try again.", "De temps à autre, Sonic Dash a besoin de se connecter à Internet pour télécharger du contenu supplémentaire.\n\nVérifie ta connexion Internet et réessaie.", "È necessario collegare Sonic Dash a internet per poter scaricare il contenuto aggiuntivo.\n\nControlla la connessione a internet e riprova.", "Sonic Dash muss gelegentlich eine Internetverbindung herstellen, um zusätzliche Inhalte herunterzuladen.\n\nBitte überprüfe deine Internetverbindung und versuche es erneut.", "En ocasiones, Sonic Dash necesita conectarse a internet para descargar contenido adicional.\n\nComprueba tu conexión a internet e inténtalo de nuevo.", "Ocasionalmente o Sonic Dash precisa se conectar à Internet para baixar conteúdo adicional.\n\nVerifique a sua conexão com a Internet e tente novamente.", "Иногда Sonic Dash требуется подключение к Интернету для загрузки дополнительного содержания.\n\nПожалуйста, проверьте подключение и попробуйте еще раз.", "Occasionally Sonic Dash needs to connect to the Internet to download additional content.\n\nPlease check your internet connection and try again.", "Occasionally Sonic Dash needs to connect to the Internet to download additional content.\n\nPlease check your internet connection and try again.",
		"Occasionally Sonic Dash needs to connect to the Internet to download additional content.\n\nPlease check your internet connection and try again."
	};

	private readonly string[] OSWarningButtonText = new string[11]
	{
		"OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK", "OK",
		"OK"
	};

	public bool ShowDownloadWarning
	{
		set
		{
			ShowDelayedWarning();
		}
	}

	public bool WaitingForConnection
	{
		set
		{
			ConnectionNotAvailable(value);
		}
	}

	public bool DownloadingContent
	{
		set
		{
			StartingToDownloadContent(value);
		}
	}

	public int CurrentBundleIndex
	{
		set
		{
			UpdateCurentBundleCount(value);
		}
	}

	public float DownloadProgress
	{
		set
		{
			UpdateDownloadProgress(value);
		}
	}

	public int BundleDownloadCount { get; set; }

	private void Start()
	{
		StopAlphaTween(m_waitingForConnectionGroup);
		StopAlphaTween(m_downloadingGroup);
	}

	private void ConnectionNotAvailable(bool connectionOut)
	{
		if (connectionOut)
		{
			StopAlphaTween(m_downloadingGroup);
			StartAlphaTween(m_waitingForConnectionGroup);
			m_waitingForConnectionText.text = GetLanguageString(WaitingForConnectionText);
		}
		else
		{
			StopAlphaTween(m_waitingForConnectionGroup);
		}
	}

	private void StartingToDownloadContent(bool downloading)
	{
		if (downloading)
		{
			StartAlphaTween(m_downloadingGroup);
			DownloadProgress = 0f;
			CurrentBundleIndex = 0;
			m_bundleDownloadText.text = GetLanguageString(DownloadingContentText);
		}
		else
		{
			StartCoroutine(DelayAlphaTweenAction(m_downloadingGroup));
		}
	}

	private void UpdateCurentBundleCount(int currentIndex)
	{
		int num = Math.Min(currentIndex + 1, BundleDownloadCount);
		string languageString = GetLanguageString(RemainingItemsText);
		string text = string.Format(languageString, num, BundleDownloadCount);
		m_bundleCountText.text = text;
	}

	private void UpdateDownloadProgress(float progress)
	{
		float sliderValue = Mathf.Clamp(progress, 0f, 1f);
		m_bundleDownloadProgress.sliderValue = sliderValue;
	}

	private void ShowDelayedWarning()
	{
		if (!m_delayedWarningShown)
		{
			string languageString = GetLanguageString(OSWarningBodyText);
			string languageString2 = GetLanguageString(OSWarningButtonText);
			SLPlugin.DisplayUIAlertView(string.Empty, languageString, languageString2);
			m_delayedWarningShown = true;
		}
	}

	private void StartAlphaTween(GameObject rootObject)
	{
		TweenAlpha[] componentsInChildren = rootObject.GetComponentsInChildren<TweenAlpha>(includeInactive: true);
		if (componentsInChildren != null)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
	}

	private void StopAlphaTween(GameObject rootObject)
	{
		TweenAlpha[] componentsInChildren = rootObject.GetComponentsInChildren<TweenAlpha>(includeInactive: true);
		if (componentsInChildren == null)
		{
			return;
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
			UIWidget component = componentsInChildren[i].GetComponent<UIWidget>();
			if (component != null)
			{
				component.alpha = 0f;
			}
		}
	}

	private IEnumerator DelayAlphaTweenAction(GameObject rootObject)
	{
		yield return new WaitForSeconds(1f);
		StopAlphaTween(rootObject);
	}

	private string GetLanguageString(string[] languageList)
	{
		Language.ID language = Language.GetLanguage();
		return languageList[(int)language];
	}
}
