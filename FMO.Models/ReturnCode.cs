
using FMO.Models;
using System.ComponentModel;

namespace FMO.Trustee;

[TypeConverter(nameof(EnumDescriptionTypeConverter))]
public enum ReturnCode
{
    Success,


    /// <summary>
    /// �ɹ�������������
    /// </summary>
    //NotFinished,

    FailedBegin,

    Unknown,
    /// <summary>
    /// �����ǿհ׵�
    /// </summary>
    EmptyResponse,

    /// <summary>
    /// �����쳣
    /// </summary>
    DataIsNotWellFormed,

    /// <summary>
    /// ���ص�json�޷����������ɶ�Ӧ����
    /// </summary>
    JsonNotPairToEntity,


    /// <summary>
    /// δʵ�ֵĽӿ�
    /// </summary>
    NotImplemented,

    /// <summary>
    /// ��������
    /// </summary>
    Network,

    /// </summary>
    [Description("��Ч�������ļ�")]
    ConfigInvalid,

    /// <summary>
    /// ��ʱ
    /// </summary>
    [Description("��ʱ")]
    TimeOut,

    /// <summary>
    /// �ӿڲ�����
    /// </summary>
    [Description("�ӿڲ�����")]
    InterfaceUnavailable,

    /// <summary>
    /// ��Ȩ����
    /// </summary>
    [Description("��Ȩ����")]
    AccessDenied,

    /// <summary>
    /// ip ���ڰ�������
    /// </summary>
    [Description("��IP�޷�����")]InvalidIP,

    /// <summary>
    /// �������Ϸ�
    /// </summary>
    [Description("�������Ϸ�")]
    ParameterInvalid,


    /// <summary>
    /// �����֤ʧ��
    /// </summary>
    [Description("�����֤ʧ��")] 
    IdentitifyFailed,


    /// <summary>
    /// ��������
    /// </summary>
    [Description("��������")]TrafficLimit,





    #region ���Ž�Ͷ�ض�
    CSCBegin = 1000,

    /// <summary>
    /// CSC ǩ�����Ϸ�
    /// </summary>
    [Description("ǩ�����Ϸ�")]
    CSC_IlligalSign,

    /// <summary>
    /// CSC ǩ������
    /// </summary>
    [Description("ǩ������")]
    CSC_SignExpired,

    /// <summary>
    /// ȱ�ٲ���apikey
    /// </summary>
    [Description("ȱ�ٲ���apikey")]
    CSC_APIKEY,

    /// <summary>
    /// ����body���ܴ���
    /// </summary>
    [Description("����body���ܴ���")]
    CSC_BodyEncrypt,
    /// <summary>
    /// ҵ����ʧ��
    /// </summary>
    [Description("ҵ����ʧ��")]
    CSC_BusinessFailed,

    /// <summary>
    /// ϵͳ�ڲ�����
    /// </summary>
    [Description("ϵͳ�ڲ�����")]
    CSC_InternalServerError,

    /// <summary>
    /// ��������ˮ��Ϊ��
    /// </summary>
    [Description("��������ˮ��Ϊ��")]
    CSC_ThirdPartySerialNumberEmpty,

    /// <summary>
    /// ����Ϊ��
    /// </summary>
    [Description("����Ϊ��")]
    CSC_ParameterEmpty,


    /// <summary>
    /// ҵ��У�鲻ͨ��
    /// </summary>
    [Description("ҵ��У�鲻ͨ��")]
    CSC_BusinessValidationFailed,
    #endregion


    #region ����
    /// <summary>
    /// Auth/Token Ϊ��
    /// </summary> 
    [Description("Token Ϊ��")] CITICS_NoTokenOrAuth,



    /// <summary>
    /// ����ֵ���Ϸ�
    /// </summary> 
    [Description("����ֵ���Ϸ�")] CITICS_Credentials,



    /// <summary>
    /// Token ����
    /// </summary> 
    [Description("Token ����")] CITICS_Token,


    /// <summary>
    /// ��������
    /// </summary>
    [Description("��������")] CITICS_Limited, 

    #endregion


    /// <summary>
    /// ��ʼ���������ܳ���һ����
    /// </summary>
    [Description("��ʼ���������ܳ���һ����")]
    CMS_DateRangeLimitOneMonth,
}