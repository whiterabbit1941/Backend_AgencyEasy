using System;
using System.Collections.Generic;

namespace EventManagement.Dto
{
    /// <summary>
    /// EmailWhitelabel Model
    /// </summary>
    public class EmailWhitelabelDto : EmailWhitelabelAbstractBase
    {
        public Guid Id { get; set; }
        public Guid CompanyID { get; set; }
        public int DomainID { get; set; }
        public string DomainName { get; set; }
        public string CnameHost { get; set; }
        public string CnameType { get; set; }
        public string CnamePointsTo { get; set; }
        public string DomainKey1Type { get; set; }
        public string DomainKey1PointsTo { get; set; }
        public string DomainKey2Type { get; set; }
        public string DomainKey2PointsTo { get; set; }
        public bool IsVerify { get; set; }
    }
    public class DomainWhiteLabelDTO
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string subdomain { get; set; }
        public string domain { get; set; }
        public List<string> domains { get; set; }
        public string username { get; set; }
        public List<object> ips { get; set; }
        public bool custom_spf { get; set; }
        public bool @default { get; set; }
        public bool legacy { get; set; }
        public bool automatic_security { get; set; }
        public bool valid { get; set; }
        public Dns dns { get; set; }
        public ValidationResults validation_results { get; set; }
    }
    public class MailCname
    {
        public bool valid { get; set; }
        public string type { get; set; }
        public string host { get; set; }
        public string data { get; set; }
        public string reason { get; set; }
    }
    public class Dkim1
    {
        public bool valid { get; set; }
        public string type { get; set; }
        public string host { get; set; }
        public string data { get; set; }
        public string reason { get; set; }
    }
    public class Dkim2
    {
        public bool valid { get; set; }
        public string type { get; set; }
        public string host { get; set; }
        public string data { get; set; }
        public string reason { get; set; }
    }
    public class Dns
    {
        public MailCname mail_cname { get; set; }
        public Dkim1 dkim1 { get; set; }
        public Dkim2 dkim2 { get; set; }
    }
    public class ValidationResults
    {
        public MailCname mail_cname { get; set; }
        public Dkim1 dkim1 { get; set; }
        public Dkim2 dkim2 { get; set; }
    }
}
 