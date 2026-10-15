using System.Collections;
using UnityEngine;

public class CarWashController : MonoBehaviour
{
    [Header("--- 1. TỌA ĐỘ & KHUNG GANTRY ---")]
    public Transform gantryTransform;
    public Transform pointStart;            // Đầu xe
    public Transform pointEnd;              // Đuôi xe
    public float moveDuration = 8.0f;       // Thời gian di chuyển 1 chiều

    [Header("--- 2. CỤM HIỆU ỨNG (SPRAY, FOAM, DRYER) ---")]
    public GameObject sprayGroup;           // Vòi xịt nước (Bước 1, 3, 4)
    public GameObject foamGroup;            // Vòi phun bọt tuyết (Bước 2)
    public GameObject dryerGroup;           // Hệ thống quạt sấy (Bước 5)

    [Header("--- 3. BƯỚC 3: RỬA BÁNH TRƯỚC & BÁNH SAU ---")]
    public Transform wheelBrushLeft;        
    public Transform wheelBrushRight;       
    
    [Range(0.0f, 1.0f)]
    public float frontWheelRatio = 0.3f;    
    [Range(0.0f, 1.0f)]
    public float rearWheelRatio = 0.7f;     
    
    public float wheelWashDuration = 4.0f;  
    public float wheelBrushRotateSpeed = 600f;

    [Header("--- 4. BƯỚC 4: 3 CHỔI CỌ VỎ XE ---")]
    public Transform brushTop;              
    public float brushTopHighY = 2.2f;       
    public float brushTopLowY = 1.3f;        
    public float brushMoveDuration = 2.0f;
    
    [Space(5)]
    public Transform brushVerticalLeft;     
    public Transform brushVerticalRight;    
    public float bodyBrushRotateSpeed = 400f;

    [Header("--- 5. BẢNG ĐÈN LED TƯỜNG (5 BƯỚC) ---")]
    public Renderer[] stageLeds;            
    public Material ledOffMaterial;         // Màu xám/tắt (Chưa chạy)
    public Material ledRedMaterial;         // Màu ĐỎ (Đang thực hiện bước)
    public Material ledGreenMaterial;       // Màu XANH (Hoàn thành bước)

    private bool isSpinningWheelBrushes = false;
    private bool isSpinningBodyBrushes = false;

    private Coroutine activeStepCoroutine = null;

    private void Start()
    {
        StopAndHideEffect(sprayGroup);
        StopAndHideEffect(foamGroup);
        StopAndHideEffect(dryerGroup);

        SetAllLEDsOff();

        if (gantryTransform != null && pointStart != null)
        {
            Vector3 startPos = gantryTransform.position;
            startPos.z = pointStart.position.z;
            gantryTransform.position = startPos;
        }

        if (brushTop != null)
        {
            Vector3 topPos = brushTop.localPosition;
            topPos.y = brushTopHighY;
            brushTop.localPosition = topPos;
        }
    }

    private void Update()
    {
        if (isSpinningWheelBrushes)
        {
            float rotStep = wheelBrushRotateSpeed * Time.deltaTime;
            if (wheelBrushLeft) wheelBrushLeft.Rotate(Vector3.up, rotStep, Space.Self);
            if (wheelBrushRight) wheelBrushRight.Rotate(Vector3.up, -rotStep, Space.Self);
        }

        if (isSpinningBodyBrushes)
        {
            float rotStep = bodyBrushRotateSpeed * Time.deltaTime;
            if (brushTop) brushTop.Rotate(Vector3.up, rotStep, Space.Self);
            if (brushVerticalLeft) brushVerticalLeft.Rotate(Vector3.up, rotStep, Space.Self);
            if (brushVerticalRight) brushVerticalRight.Rotate(Vector3.up, rotStep, Space.Self);
        }
    }

    // =========================================================================
    // HÀM CHẠY TRỌN BỘ 5 BƯỚC TỰ ĐỘNG
    // =========================================================================
    public void StartWashProcess()
    {
        StopCurrentStep();
        activeStepCoroutine = StartCoroutine(CarWashSequenceRoutine());
    }

    private IEnumerator CarWashSequenceRoutine()
    {
        SetAllLEDsOff(); // Reset các đèn về trạng thái Tắt trước khi bắt đầu chuỗi
        yield return StartCoroutine(Step1_PreWashRoutine());
        yield return StartCoroutine(Step2_FoamRoutine());
        yield return StartCoroutine(Step3_WheelWashRoutine());
        yield return StartCoroutine(Step4_BodyScrubRoutine());
        yield return StartCoroutine(Step5_DryerRoutine());
    }

    // =========================================================================
    // HÀM CHUYỂN BƯỚC LẬP TỨC KHI BẤM NÚT (1 -> 6)
    // =========================================================================
    public void ExecuteStep(int stepIndex)
    {
        StopCurrentStep();

        switch (stepIndex)
        {
            case 1:
                activeStepCoroutine = StartCoroutine(Step1_PreWashRoutine());
                break;
            case 2:
                activeStepCoroutine = StartCoroutine(Step2_FoamRoutine());
                break;
            case 3:
                activeStepCoroutine = StartCoroutine(Step3_WheelWashRoutine());
                break;
            case 4:
                activeStepCoroutine = StartCoroutine(Step4_BodyScrubRoutine());
                break;
            case 5:
                activeStepCoroutine = StartCoroutine(Step5_DryerRoutine());
                break;
            case 6:
                // NÚT 6: RỬA TỰ ĐỘNG TRỌN GÓI 5 BƯỚC
                Debug.Log(">>> KÍCH HOẠT QUY TRÌNH RỬA TỰ ĐỘNG TRỌN GÓI (5 BƯỚC) <<<");
                activeStepCoroutine = StartCoroutine(CarWashSequenceRoutine());
                break;
            default:
                Debug.LogError("Bước rửa không hợp lệ: " + stepIndex);
                break;
        }
    }

    public void StopCurrentStep()
    {
        StopAllCoroutines();
        activeStepCoroutine = null;

        StopAndHideEffect(sprayGroup);
        StopAndHideEffect(foamGroup);
        StopAndHideEffect(dryerGroup);

        isSpinningWheelBrushes = false;
        isSpinningBodyBrushes = false;

        if (brushTop != null)
        {
            Vector3 topPos = brushTop.localPosition;
            topPos.y = brushTopHighY;
            brushTop.localPosition = topPos;
        }

        SetAllLEDsOff();
    }

    // =========================================================================
    // CÁC BƯỚC RỬA RIÊNG BIỆT (XỬ LÝ ĐÈN ĐỎ -> ĐÈN XANH)
    // =========================================================================

    // BƯỚC 1: XẢ NƯỚC RỬA SƠ BỘ
    private IEnumerator Step1_PreWashRoutine()
    {
        SetLEDState(0, ledRedMaterial); // Đang chạy: Sáng ĐỎ
        PlayEffect(sprayGroup);

        yield return MoveGantry(gantryTransform.position.z, pointEnd.position.z, moveDuration);

        StopAndHideEffect(sprayGroup);
        SetLEDState(0, ledGreenMaterial); // Hoàn thành: Đổi sang XANH
        yield return new WaitForSeconds(0.3f);
        Debug.Log(">>> BƯỚC 1 HOÀN THÀNH <<<");
    }

    // BƯỚC 2: PHUN BỌT TUYẾT PHỦ THÂN XE
    private IEnumerator Step2_FoamRoutine()
    {
        SetLEDState(1, ledRedMaterial); // Đang chạy: Sáng ĐỎ
        PlayEffect(foamGroup);

        yield return MoveGantry(gantryTransform.position.z, pointStart.position.z, moveDuration);

        StopAndHideEffect(foamGroup);
        SetLEDState(1, ledGreenMaterial); // Hoàn thành: Đổi sang XANH
        yield return new WaitForSeconds(0.3f);
        Debug.Log(">>> BƯỚC 2 HOÀN THÀNH <<<");
    }

    // BƯỚC 3: CỌ MÂM BÁNH TRƯỚC VÀ BÁNH SAU + XẢ NƯỚC
    private IEnumerator Step3_WheelWashRoutine()
    {
        SetLEDState(2, ledRedMaterial); // Đang chạy: Sáng ĐỎ

        float frontWheelZ = Mathf.Lerp(pointStart.position.z, pointEnd.position.z, frontWheelRatio);
        float rearWheelZ  = Mathf.Lerp(pointStart.position.z, pointEnd.position.z, rearWheelRatio);

        PlayEffect(sprayGroup);

        yield return MoveGantry(gantryTransform.position.z, frontWheelZ, moveDuration * 0.4f);
        isSpinningWheelBrushes = true;
        yield return ScrubWheelAtPosition(frontWheelZ, wheelWashDuration);

        yield return MoveGantry(gantryTransform.position.z, rearWheelZ, moveDuration * 0.4f);
        yield return ScrubWheelAtPosition(rearWheelZ, wheelWashDuration);

        isSpinningWheelBrushes = false;
        StopAndHideEffect(sprayGroup);

        yield return MoveGantry(gantryTransform.position.z, pointStart.position.z, moveDuration * 0.5f);
        SetLEDState(2, ledGreenMaterial); // Hoàn thành: Đổi sang XANH
        yield return new WaitForSeconds(0.3f);
        Debug.Log(">>> BƯỚC 3 HOÀN THÀNH <<<");
    }

    // BƯỚC 4: 3 CHỔI XOAY CỌ RỬA VỎ XE (2 LƯỢT KHỨ HỒI)
    private IEnumerator Step4_BodyScrubRoutine()
    {
        SetLEDState(3, ledRedMaterial); // Đang chạy: Sáng ĐỎ

        yield return MoveBrushTopY(brushTop.localPosition.y, brushTopLowY, brushMoveDuration);

        isSpinningBodyBrushes = true;
        PlayEffect(sprayGroup);

        // Chà Lượt 1: Điểm hiện tại -> Đuôi xe
        yield return MoveGantry(gantryTransform.position.z, pointEnd.position.z, moveDuration);

        // Chà Lượt 2: Đuôi xe -> Đầu xe
        yield return MoveGantry(gantryTransform.position.z, pointStart.position.z, moveDuration);

        isSpinningBodyBrushes = false;
        StopAndHideEffect(sprayGroup);

        yield return MoveBrushTopY(brushTop.localPosition.y, brushTopHighY, brushMoveDuration);
        SetLEDState(3, ledGreenMaterial); // Hoàn thành: Đổi sang XANH
        yield return new WaitForSeconds(0.3f);
        Debug.Log(">>> BƯỚC 4 HOÀN THÀNH (ĐÃ CHÀ 2 LƯỢT) <<<");
    }

    // BƯỚC 5: THỔI KHÔ BẰNG QUẠT SẤY
    private IEnumerator Step5_DryerRoutine()
    {
        SetLEDState(4, ledRedMaterial); // Đang chạy: Sáng ĐỎ
        PlayEffect(dryerGroup);

        yield return MoveGantry(gantryTransform.position.z, pointStart.position.z, moveDuration);

        StopAndHideEffect(dryerGroup);
        SetLEDState(4, ledGreenMaterial); // Hoàn thành: Đổi sang XANH
        yield return new WaitForSeconds(0.3f);
        Debug.Log(">>> BƯỚC 5 HOÀN THÀNH <<<");
    }

    // =========================================================================
    // TÍNH TOÁN DI CHUYỂN GANTRY
    // =========================================================================
    private IEnumerator MoveGantry(float startZ, float targetZ, float duration)
    {
        if (gantryTransform == null || pointStart == null || pointEnd == null) yield break;

        float totalFullDistance = Mathf.Abs(pointEnd.position.z - pointStart.position.z);
        float distanceToTravel = Mathf.Abs(targetZ - startZ);

        float actualDuration = (totalFullDistance > 0.001f) 
            ? duration * (distanceToTravel / totalFullDistance) 
            : duration;

        if (actualDuration <= 0.01f)
        {
            Vector3 finalPos = gantryTransform.position;
            finalPos.z = targetZ;
            gantryTransform.position = finalPos;
            yield break;
        }

        float elapsed = 0f;
        Vector3 pos = gantryTransform.position;

        while (elapsed < actualDuration)
        {
            elapsed += Time.deltaTime;
            float newZ = Mathf.Lerp(startZ, targetZ, elapsed / actualDuration);
            pos.z = newZ;
            gantryTransform.position = pos;
            yield return null;
        }

        pos.z = targetZ;
        gantryTransform.position = pos;
    }

    private IEnumerator ScrubWheelAtPosition(float centerZ, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float offset = Mathf.Sin(timer * 8f) * 0.12f;
            Vector3 pos = gantryTransform.position;
            pos.z = centerZ + offset;
            gantryTransform.position = pos;
            yield return null;
        }
    }

    private IEnumerator MoveBrushTopY(float startY, float targetY, float duration)
    {
        if (brushTop == null) yield break;

        float elapsed = 0f;
        Vector3 localPos = brushTop.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            localPos.y = Mathf.Lerp(startY, targetY, elapsed / duration);
            brushTop.localPosition = localPos;
            yield return null;
        }

        localPos.y = targetY;
        brushTop.localPosition = localPos;
    }

    private void PlayEffect(GameObject objGroup)
    {
        if (objGroup != null)
        {
            objGroup.SetActive(true);
            ParticleSystem ps = objGroup.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
        }
    }

    private void StopAndHideEffect(GameObject objGroup)
    {
        if (objGroup != null)
        {
            ParticleSystem ps = objGroup.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            objGroup.SetActive(false);
        }
    }

    // =========================================================================
    // QUẢN LÝ ĐÈN LED
    // =========================================================================
    private void SetLEDState(int index, Material mat)
    {
        if (stageLeds != null && index >= 0 && index < stageLeds.Length)
        {
            if (stageLeds[index] != null && mat != null)
            {
                stageLeds[index].material = mat;
            }
        }
    }

    public void SetAllLEDsOff()
    {
        if (stageLeds == null) return;
        foreach (var led in stageLeds)
        {
            if (led != null && ledOffMaterial != null)
            {
                led.material = ledOffMaterial;
            }
        }
    }
}