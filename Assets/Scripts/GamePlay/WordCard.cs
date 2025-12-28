using TMPro;
using UnityEngine;

public class WordCard : MonoBehaviour
{
    public WordData data;
    [SerializeField] private TextMeshProUGUI label;
    private GameManager manager;

    public void Setup(WordData wordData, GameManager gameManager, int fontSize)
    {
        data=wordData;
        manager=gameManager;
        label.text=wordData.text;
        label.fontSize=fontSize;

        // Visual Hint: Color Coding
        // Business: Blue, Casual: Orange, Risk: Purple, Neutral: Black
        float b = wordData.businessScore;
        float c = wordData.casualScore;
        float r = wordData.riskScore;

        Color textColor = Color.black;

        // 単純な比較で一番高い属性の色にする。ただし閾値(20)以上の場合のみ
        float threshold = 20f;
        float maxVal = Mathf.Max(b, c, r);

        if (maxVal > threshold)
        {
            if (Mathf.Approximately(maxVal, r))
            {
                // Risk (Purple)
                ColorUtility.TryParseHtmlString("#D040FF", out textColor);
            }
            else if (Mathf.Approximately(maxVal, b))
            {
                // Business (Blue)
                ColorUtility.TryParseHtmlString("#0070DD", out textColor);
            }
            else if (Mathf.Approximately(maxVal, c))
            {
                // Casual (Orange)
                ColorUtility.TryParseHtmlString("#EE7700", out textColor);
            }
        }
        
        label.color = textColor;
    }
    public void OnClick()
    {
        manager.OnWordSelected(this);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
