using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EventManagement.Dto
{
    public class DataForSeoResponse
    {
        public string Status { get; set; }
        public string ResultsTime { get; set; }
        public long ResultsCount { get; set; }
        //public List<Error> Error { get; set; }
        public List<Result> Results { get; set; }



    }
    //public class Error
    //{
    //    public int Code { get; set; }
    //    public string Message { get; set; }
    //}


    public class AuditData
    {
        public List<Summary> Summary { get; set; }

        public List<Page> Pages { get; set; }

        public List<BrokenPage> BrokenPages { get; set; }

        public List<DuplicatesPage> Duplicates { get; set; }
    }

    public class Result
    {
        public long Post_Id { get; set; }
        public string post_Site { get; set; }
        public long Task_Id { get; set; }
        public string Status { get; set; }
        public List<Summary> Summary { get; set; }

        public List<Page> Pages { get; set; }

        public List<BrokenPage> BrokenPages { get; set; }

        public List<DuplicatesPage> Duplicates { get; set; }

        public List<LinksTo> LinksTo { get; set; }



    }

    public class DuplicatesPage {

        [JsonProperty("accumulator")]
        public string Accumulator { get; set; }

        public List<DuplicatePage> Pages {get;set;}
    }

    public class Summary
    {
        [JsonProperty("absent_doctype")]
        public long AbsentDoctype { get; set; }

        [JsonProperty("absent_encoding_meta_tag")]
        public long AbsentEncodingMetaTag { get; set; }

        [JsonProperty("absent_h1_tags")]
        public long AbsentH1Tags { get; set; }

        [JsonProperty("canonical_another")]
        public long CanonicalAnother { get; set; }

        [JsonProperty("canonical_recursive")]
        public long CanonicalRecursive { get; set; }

        [JsonProperty("cms")]
        public object Cms { get; set; }

        [JsonProperty("compression_disabled")]
        public long CompressionDisabled { get; set; }

        [JsonProperty("content_invalid_rate")]
        public long ContentInvalidRate { get; set; }

        [JsonProperty("content_invalid_size")]
        public long ContentInvalidSize { get; set; }

        [JsonProperty("content_readability_bad")]
        public long ContentReadabilityBad { get; set; }

        [JsonProperty("crawl_end")]
        public DateTimeOffset CrawlEnd { get; set; }

        [JsonProperty("crawl_start")]
        public DateTimeOffset CrawlStart { get; set; }

        [JsonProperty("deprecated_html_tags")]
        public long DeprecatedHtmlTags { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("duplicate_meta_descriptions")]
        public long DuplicateMetaDescriptions { get; set; }

        [JsonProperty("duplicate_meta_tags")]
        public long DuplicateMetaTags { get; set; }

        [JsonProperty("duplicate_pages")]
        public long DuplicatePages { get; set; }

        [JsonProperty("duplicate_titles")]
        public long DuplicateTitles { get; set; }

        [JsonProperty("favicon_invalid")]
        public long FaviconInvalid { get; set; }

        [JsonProperty("have_robots")]
        public bool HaveRobots { get; set; }

        [JsonProperty("have_sitemap")]
        public bool HaveSitemap { get; set; }

        [JsonProperty("images_invalid_alt")]
        public long ImagesInvalidAlt { get; set; }

        [JsonProperty("images_invalid_title")]
        public long ImagesInvalidTitle { get; set; }

        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("links_broken")]
        public long LinksBroken { get; set; }

        [JsonProperty("links_external")]
        public long LinksExternal { get; set; }

        [JsonProperty("links_internal")]
        public long LinksInternal { get; set; }

        [JsonProperty("meta_description_empty")]
        public long MetaDescriptionEmpty { get; set; }

        [JsonProperty("meta_description_inappropriate")]
        public long MetaDescriptionInappropriate { get; set; }

        [JsonProperty("meta_keywords_empty")]
        public long MetaKeywordsEmpty { get; set; }

        [JsonProperty("meta_keywords_inappropriate")]
        public long MetaKeywordsInappropriate { get; set; }

        [JsonProperty("pages_broken")]
        public long PagesBroken { get; set; }

        [JsonProperty("pages_http")]
        public long PagesHttp { get; set; }

        [JsonProperty("pages_https")]
        public long PagesHttps { get; set; }

        [JsonProperty("pages_invalid_size")]
        public long PagesInvalidSize { get; set; }

        [JsonProperty("pages_non_www")]
        public long PagesNonWww { get; set; }

        [JsonProperty("pages_total")]
        public long PagesTotal { get; set; }

        [JsonProperty("pages_with_flash")]
        public long PagesWithFlash { get; set; }

        [JsonProperty("pages_with_frame")]
        public long PagesWithFrame { get; set; }

        [JsonProperty("pages_with_lorem_ipsum")]
        public long PagesWithLoremIpsum { get; set; }

        [JsonProperty("pages_www")]
        public long PagesWww { get; set; }

        [JsonProperty("response_code_1xx")]
        public long ResponseCode1Xx { get; set; }

        [JsonProperty("response_code_2xx")]
        public long ResponseCode2Xx { get; set; }

        [JsonProperty("response_code_3xx")]
        public long ResponseCode3Xx { get; set; }

        [JsonProperty("response_code_4xx")]
        public long ResponseCode4Xx { get; set; }

        [JsonProperty("response_code_5xx")]
        public long ResponseCode5Xx { get; set; }

        [JsonProperty("response_code_other")]
        public long ResponseCodeOther { get; set; }

        [JsonProperty("seo_friendly_url")]
        public long SeoFriendlyUrl { get; set; }

        [JsonProperty("seo_non_friendly_url")]
        public long SeoNonFriendlyUrl { get; set; }

        [JsonProperty("server")]
        public string Server { get; set; }

        [JsonProperty("ssl")]
        public bool Ssl { get; set; }

        [JsonProperty("ssl_certificate_expiration")]
        public DateTimeOffset SslCertificateExpiration { get; set; }

        [JsonProperty("ssl_certificate_hash_algorithm")]
        public string SslCertificateHashAlgorithm { get; set; }

        [JsonProperty("ssl_certificate_issuer")]
        public string SslCertificateIssuer { get; set; }

        [JsonProperty("ssl_certificate_subject")]
        public string SslCertificateSubject { get; set; }

        [JsonProperty("ssl_certificate_valid")]
        public bool SslCertificateValid { get; set; }

        [JsonProperty("ssl_certificate_x509_version")]
        public long SslCertificateX509Version { get; set; }

        [JsonProperty("start_page_has_deny_flag")]
        public bool StartPageHasDenyFlag { get; set; }

        [JsonProperty("string_containment_check")]
        public long StringContainmentCheck { get; set; }

        [JsonProperty("test_canonicalization")]
        public long TestCanonicalization { get; set; }

        [JsonProperty("test_directory_browsing")]
        public bool TestDirectoryBrowsing { get; set; }

        [JsonProperty("test_server_signature")]
        public bool TestServerSignature { get; set; }

        [JsonProperty("test_trash_page")]
        public long TestTrashPage { get; set; }

        [JsonProperty("time_load_high")]
        public long TimeLoadHigh { get; set; }

        [JsonProperty("time_waiting_high")]
        public long TimeWaitingHigh { get; set; }

        [JsonProperty("title_duplicate_tag")]
        public long TitleDuplicateTag { get; set; }

        [JsonProperty("title_empty")]
        public long TitleEmpty { get; set; }

        [JsonProperty("title_inappropriate")]
        public long TitleInappropriate { get; set; }

        [JsonProperty("title_long")]
        public long TitleLong { get; set; }

        [JsonProperty("title_short")]
        public long TitleShort { get; set; }

        [JsonProperty("www")]
        public bool Www { get; set; }

    }

    public class Page
    {

        [JsonProperty("address_full")]
        public Uri AddressFull { get; set; }

        [JsonProperty("address_relative")]
        public string AddressRelative { get; set; }

        [JsonProperty("canonical_another")]
        public bool CanonicalAnother { get; set; }

        [JsonProperty("canonical_page")]
        public object CanonicalPage { get; set; }

        [JsonProperty("canonical_page_recursive")]
        public string CanonicalPageRecursive { get; set; }

        [JsonProperty("content_charset")]
        public long ContentCharset { get; set; }

        [JsonProperty("content_count_words")]
        public long ContentCountWords { get; set; }

        [JsonProperty("content_encoding")]
        public string ContentEncoding { get; set; }

        [JsonProperty("content_readability_ari")]
        public double ContentReadabilityAri { get; set; }

        [JsonProperty("content_readability_coleman_liau")]
        public double ContentReadabilityColemanLiau { get; set; }

        [JsonProperty("content_readability_dale_chall")]
        public double ContentReadabilityDaleChall { get; set; }

        [JsonProperty("content_readability_flesh_kincaid")]
        public double ContentReadabilityFleshKincaid { get; set; }

        [JsonProperty("content_readability_smog")]
        public double ContentReadabilitySmog { get; set; }

        [JsonProperty("crawl_depth")]
        public long CrawlDepth { get; set; }

        [JsonProperty("crawl_end")]
        public DateTimeOffset CrawlEnd { get; set; }

        [JsonProperty("crawled")]
        public bool Crawled { get; set; }

        [JsonProperty("deprecated_html_tags")]
        public List<string> DeprecatedHtmlTags { get; set; }

        [JsonProperty("duplicate_meta_tags")]
        public List<object> DuplicateMetaTags { get; set; }

        [JsonProperty("favicon")]
        public string Favicon { get; set; }

        [JsonProperty("h1_count")]
        public long H1Count { get; set; }

        [JsonProperty("h2_count")]
        public long H2Count { get; set; }

        [JsonProperty("h3_count")]
        public long H3Count { get; set; }

        [JsonProperty("have_deprecated_tags")]
        public bool HaveDeprecatedTags { get; set; }

        [JsonProperty("have_doctype")]
        public bool HaveDoctype { get; set; }

        [JsonProperty("have_enc_meta_tag")]
        public bool HaveEncMetaTag { get; set; }

        [JsonProperty("have_flash")]
        public bool HaveFlash { get; set; }

        [JsonProperty("have_frame")]
        public bool HaveFrame { get; set; }

        [JsonProperty("have_lorem_ipsum")]
        public bool HaveLoremIpsum { get; set; }

        [JsonProperty("have_meta_description_duplicates")]
        public bool HaveMetaDescriptionDuplicates { get; set; }

        [JsonProperty("have_page_duplicates")]
        public bool HavePageDuplicates { get; set; }

        [JsonProperty("have_recursive_canonical")]
        public bool HaveRecursiveCanonical { get; set; }

        [JsonProperty("have_redirect")]
        public bool HaveRedirect { get; set; }

        [JsonProperty("have_title_duplicates")]
        public bool HaveTitleDuplicates { get; set; }

        [JsonProperty("images_count")]
        public long ImagesCount { get; set; }

        [JsonProperty("images_invalid_alt")]
        public long ImagesInvalidAlt { get; set; }

        [JsonProperty("images_invalid_title")]
        public long ImagesInvalidTitle { get; set; }

        [JsonProperty("links_broken")]
        public long LinksBroken { get; set; }

        [JsonProperty("links_external")]
        public long LinksExternal { get; set; }

        [JsonProperty("links_internal")]
        public long LinksInternal { get; set; }

        [JsonProperty("links_referring")]
        public long LinksReferring { get; set; }

        [JsonProperty("meta_description")]
        public string MetaDescription { get; set; }

        [JsonProperty("meta_description_consistency")]
        public double MetaDescriptionConsistency { get; set; }

        [JsonProperty("meta_description_length")]
        public long MetaDescriptionLength { get; set; }

        [JsonProperty("meta_keywords")]
        public string MetaKeywords { get; set; }

        [JsonProperty("meta_keywords_consistency")]
        public long MetaKeywordsConsistency { get; set; }

        [JsonProperty("page_allowed")]
        public bool PageAllowed { get; set; }

        [JsonProperty("page_redirect")]
        public object PageRedirect { get; set; }

        [JsonProperty("page_size")]
        public long PageSize { get; set; }

        [JsonProperty("plain_text_rate")]
        public double PlainTextRate { get; set; }

        [JsonProperty("plain_text_size")]
        public long PlainTextSize { get; set; }

        [JsonProperty("relative_path_length")]
        public long RelativePathLength { get; set; }

        [JsonProperty("response_code")]
        public long ResponseCode { get; set; }

        [JsonProperty("seo_friendly_url")]
        public bool SeoFriendlyUrl { get; set; }

        [JsonProperty("seo_friendly_url_characters_check")]
        public bool SeoFriendlyUrlCharactersCheck { get; set; }

        [JsonProperty("seo_friendly_url_dynamic_check")]
        public bool SeoFriendlyUrlDynamicCheck { get; set; }

        [JsonProperty("seo_friendly_url_keywords_check")]
        public bool SeoFriendlyUrlKeywordsCheck { get; set; }

        [JsonProperty("seo_friendly_url_relative_length_check")]
        public bool SeoFriendlyUrlRelativeLengthCheck { get; set; }

        [JsonProperty("ssl")]
        public bool Ssl { get; set; }

        [JsonProperty("ssl_handshake_time")]
        public long SslHandshakeTime { get; set; }

        [JsonProperty("string_containment_check")]
        public bool StringContainmentCheck { get; set; }

        [JsonProperty("time_connection")]
        public long TimeConnection { get; set; }

        [JsonProperty("time_download")]
        public long TimeDownload { get; set; }

        [JsonProperty("time_sending_request")]
        public long TimeSendingRequest { get; set; }

        [JsonProperty("time_total_load")]
        public long TimeTotalLoad { get; set; }

        [JsonProperty("time_waiting")]
        public long TimeWaiting { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_consistency")]
        public long TitleConsistency { get; set; }

        [JsonProperty("title_duplicate_tag")]
        public bool TitleDuplicateTag { get; set; }

        [JsonProperty("title_length")]
        public long TitleLength { get; set; }

        [JsonProperty("www")]
        public bool Www { get; set; }
    }

    public class BrokenPage
    {
        [JsonProperty("address_full")]
        public Uri AddressFull { get; set; }

        [JsonProperty("address_relative")]
        public string AddressRelative { get; set; }

        [JsonProperty("canonical_another")]
        public bool CanonicalAnother { get; set; }

        [JsonProperty("canonical_page")]
        public object CanonicalPage { get; set; }

        [JsonProperty("canonical_page_recursive")]
        public string CanonicalPageRecursive { get; set; }

        [JsonProperty("content_charset")]
        public long ContentCharset { get; set; }

        [JsonProperty("content_count_words")]
        public long ContentCountWords { get; set; }

        [JsonProperty("content_encoding")]
        public string ContentEncoding { get; set; }

        [JsonProperty("content_readability_ari")]
        public long ContentReadabilityAri { get; set; }

        [JsonProperty("content_readability_coleman_liau")]
        public long ContentReadabilityColemanLiau { get; set; }

        [JsonProperty("content_readability_dale_chall")]
        public long ContentReadabilityDaleChall { get; set; }

        [JsonProperty("content_readability_flesh_kincaid")]
        public long ContentReadabilityFleshKincaid { get; set; }

        [JsonProperty("content_readability_smog")]
        public long ContentReadabilitySmog { get; set; }

        [JsonProperty("crawl_depth")]
        public long CrawlDepth { get; set; }

        [JsonProperty("crawl_end")]
        public DateTimeOffset CrawlEnd { get; set; }

        [JsonProperty("crawled")]
        public bool Crawled { get; set; }

        [JsonProperty("deprecated_html_tags")]
        public List<object> DeprecatedHtmlTags { get; set; }

        [JsonProperty("duplicate_meta_tags")]
        public List<object> DuplicateMetaTags { get; set; }

        [JsonProperty("favicon")]
        public string Favicon { get; set; }

        [JsonProperty("h1_count")]
        public long H1Count { get; set; }

        [JsonProperty("h2_count")]
        public long H2Count { get; set; }

        [JsonProperty("h3_count")]
        public long H3Count { get; set; }

        [JsonProperty("have_deprecated_tags")]
        public bool HaveDeprecatedTags { get; set; }

        [JsonProperty("have_doctype")]
        public bool HaveDoctype { get; set; }

        [JsonProperty("have_enc_meta_tag")]
        public bool HaveEncMetaTag { get; set; }

        [JsonProperty("have_flash")]
        public bool HaveFlash { get; set; }

        [JsonProperty("have_frame")]
        public bool HaveFrame { get; set; }

        [JsonProperty("have_lorem_ipsum")]
        public bool HaveLoremIpsum { get; set; }

        [JsonProperty("have_meta_description_duplicates")]
        public bool HaveMetaDescriptionDuplicates { get; set; }

        [JsonProperty("have_page_duplicates")]
        public bool HavePageDuplicates { get; set; }

        [JsonProperty("have_recursive_canonical")]
        public bool HaveRecursiveCanonical { get; set; }

        [JsonProperty("have_redirect")]
        public bool HaveRedirect { get; set; }

        [JsonProperty("have_title_duplicates")]
        public bool HaveTitleDuplicates { get; set; }

        [JsonProperty("images_count")]
        public long ImagesCount { get; set; }

        [JsonProperty("images_invalid_alt")]
        public long ImagesInvalidAlt { get; set; }

        [JsonProperty("images_invalid_title")]
        public long ImagesInvalidTitle { get; set; }

        [JsonProperty("links_broken")]
        public long LinksBroken { get; set; }

        [JsonProperty("links_external")]
        public long LinksExternal { get; set; }

        [JsonProperty("links_internal")]
        public long LinksInternal { get; set; }

        [JsonProperty("links_referring")]
        public long LinksReferring { get; set; }

        [JsonProperty("meta_description")]
        public object MetaDescription { get; set; }

        [JsonProperty("meta_description_consistency")]
        public long MetaDescriptionConsistency { get; set; }

        [JsonProperty("meta_description_length")]
        public long MetaDescriptionLength { get; set; }

        [JsonProperty("meta_keywords")]
        public string MetaKeywords { get; set; }

        [JsonProperty("meta_keywords_consistency")]
        public long MetaKeywordsConsistency { get; set; }

        [JsonProperty("page_allowed")]
        public bool PageAllowed { get; set; }

        [JsonProperty("page_redirect")]
        public object PageRedirect { get; set; }

        [JsonProperty("page_size")]
        public long PageSize { get; set; }

        [JsonProperty("plain_text_rate")]
        public long PlainTextRate { get; set; }

        [JsonProperty("plain_text_size")]
        public long PlainTextSize { get; set; }

        [JsonProperty("relative_path_length")]
        public long RelativePathLength { get; set; }

        [JsonProperty("response_code")]
        public long ResponseCode { get; set; }

        [JsonProperty("seo_friendly_url")]
        public bool SeoFriendlyUrl { get; set; }

        [JsonProperty("seo_friendly_url_characters_check")]
        public bool SeoFriendlyUrlCharactersCheck { get; set; }

        [JsonProperty("seo_friendly_url_dynamic_check")]
        public bool SeoFriendlyUrlDynamicCheck { get; set; }

        [JsonProperty("seo_friendly_url_keywords_check")]
        public bool SeoFriendlyUrlKeywordsCheck { get; set; }

        [JsonProperty("seo_friendly_url_relative_length_check")]
        public bool SeoFriendlyUrlRelativeLengthCheck { get; set; }

        [JsonProperty("ssl")]
        public bool Ssl { get; set; }

        [JsonProperty("ssl_handshake_time")]
        public long SslHandshakeTime { get; set; }

        [JsonProperty("string_containment_check")]
        public bool StringContainmentCheck { get; set; }

        [JsonProperty("time_connection")]
        public long TimeConnection { get; set; }

        [JsonProperty("time_download")]
        public long TimeDownload { get; set; }

        [JsonProperty("time_total_load")]
        public long TimeTotalLoad { get; set; }

        [JsonProperty("time_sending_request")]
        public long TimeSendingRequest { get; set; }

        [JsonProperty("time_waiting")]
        public long TimeWaiting { get; set; }

        [JsonProperty("title")]
        public object Title { get; set; }

        [JsonProperty("title_consistency")]
        public long TitleConsistency { get; set; }

        [JsonProperty("title_length")]
        public long TitleLength { get; set; }

        [JsonProperty("www")]
        public bool Www { get; set; }

    }

    public class DuplicatePage
    {

        [JsonProperty("accumulator")]
        public string Accumulator { get; set; }

        [JsonProperty("address_full")]
        public Uri AddressFull { get; set; }

        [JsonProperty("address_relative")]
        public string AddressRelative { get; set; }

        [JsonProperty("canonical_another")]
        public bool CanonicalAnother { get; set; }

        [JsonProperty("canonical_page")]
        public object CanonicalPage { get; set; }

        [JsonProperty("canonical_page_recursive")]
        public string CanonicalPageRecursive { get; set; }

        [JsonProperty("content_charset")]
        public long ContentCharset { get; set; }

        [JsonProperty("content_count_words")]
        public long ContentCountWords { get; set; }

        [JsonProperty("content_encoding")]
        public string ContentEncoding { get; set; }

        [JsonProperty("content_readability_ari")]
        public double ContentReadabilityAri { get; set; }

        [JsonProperty("content_readability_coleman_liau")]
        public double ContentReadabilityColemanLiau { get; set; }

        [JsonProperty("content_readability_dale_chall")]
        public double ContentReadabilityDaleChall { get; set; }

        [JsonProperty("content_readability_flesh_kincaid")]
        public double ContentReadabilityFleshKincaid { get; set; }

        [JsonProperty("content_readability_smog")]
        public double ContentReadabilitySmog { get; set; }

        [JsonProperty("crawl_depth")]
        public long CrawlDepth { get; set; }

        [JsonProperty("crawl_end")]
        public DateTimeOffset CrawlEnd { get; set; }

        [JsonProperty("crawled")]
        public bool Crawled { get; set; }

        [JsonProperty("deprecated_html_tags")]
        public List<object> DeprecatedHtmlTags { get; set; }

        [JsonProperty("duplicate_meta_tags")]
        public List<object> DuplicateMetaTags { get; set; }

        [JsonProperty("favicon")]
        public string Favicon { get; set; }

        [JsonProperty("h1_count")]
        public long H1Count { get; set; }

        [JsonProperty("h2_count")]
        public long H2Count { get; set; }

        [JsonProperty("h3_count")]
        public long H3Count { get; set; }

        [JsonProperty("have_deprecated_tags")]
        public bool HaveDeprecatedTags { get; set; }

        [JsonProperty("have_doctype")]
        public bool HaveDoctype { get; set; }

        [JsonProperty("have_enc_meta_tag")]
        public bool HaveEncMetaTag { get; set; }

        [JsonProperty("have_flash")]
        public bool HaveFlash { get; set; }

        [JsonProperty("have_frame")]
        public bool HaveFrame { get; set; }

        [JsonProperty("have_lorem_ipsum")]
        public bool HaveLoremIpsum { get; set; }

        [JsonProperty("have_meta_description_duplicates")]
        public bool HaveMetaDescriptionDuplicates { get; set; }

        [JsonProperty("have_page_duplicates")]
        public bool HavePageDuplicates { get; set; }

        [JsonProperty("have_recursive_canonical")]
        public bool HaveRecursiveCanonical { get; set; }

        [JsonProperty("have_redirect")]
        public bool HaveRedirect { get; set; }

        [JsonProperty("have_title_duplicates")]
        public bool HaveTitleDuplicates { get; set; }

        [JsonProperty("images_count")]
        public long ImagesCount { get; set; }

        [JsonProperty("images_invalid_alt")]
        public long ImagesInvalidAlt { get; set; }

        [JsonProperty("images_invalid_title")]
        public long ImagesInvalidTitle { get; set; }

        [JsonProperty("links_broken")]
        public long LinksBroken { get; set; }

        [JsonProperty("links_external")]
        public long LinksExternal { get; set; }

        [JsonProperty("links_internal")]
        public long LinksInternal { get; set; }

        [JsonProperty("links_referring")]
        public long LinksReferring { get; set; }

        [JsonProperty("meta_description")]
        public string MetaDescription { get; set; }

        [JsonProperty("meta_description_consistency")]
        public long MetaDescriptionConsistency { get; set; }

        [JsonProperty("meta_description_length")]
        public long MetaDescriptionLength { get; set; }

        [JsonProperty("meta_keywords")]
        public string MetaKeywords { get; set; }

        [JsonProperty("meta_keywords_consistency")]
        public long MetaKeywordsConsistency { get; set; }

        [JsonProperty("page_allowed")]
        public bool PageAllowed { get; set; }

        [JsonProperty("page_redirect")]
        public object PageRedirect { get; set; }

        [JsonProperty("page_size")]
        public long PageSize { get; set; }

        [JsonProperty("plain_text_rate")]
        public double PlainTextRate { get; set; }

        [JsonProperty("plain_text_size")]
        public long PlainTextSize { get; set; }

        [JsonProperty("relative_path_length")]
        public long RelativePathLength { get; set; }

        [JsonProperty("response_code")]
        public long ResponseCode { get; set; }

        [JsonProperty("seo_friendly_url")]
        public bool SeoFriendlyUrl { get; set; }

        [JsonProperty("seo_friendly_url_characters_check")]
        public bool SeoFriendlyUrlCharactersCheck { get; set; }

        [JsonProperty("seo_friendly_url_dynamic_check")]
        public bool SeoFriendlyUrlDynamicCheck { get; set; }

        [JsonProperty("seo_friendly_url_keywords_check")]
        public bool SeoFriendlyUrlKeywordsCheck { get; set; }

        [JsonProperty("seo_friendly_url_relative_length_check")]
        public bool SeoFriendlyUrlRelativeLengthCheck { get; set; }

        [JsonProperty("ssl")]
        public bool Ssl { get; set; }

        [JsonProperty("ssl_handshake_time")]
        public long SslHandshakeTime { get; set; }

        [JsonProperty("string_containment_check")]
        public bool StringContainmentCheck { get; set; }

        [JsonProperty("time_connection")]
        public long TimeConnection { get; set; }

        [JsonProperty("time_download")]
        public long TimeDownload { get; set; }

        [JsonProperty("time_sending_request")]
        public long TimeSendingRequest { get; set; }

        [JsonProperty("time_total_load")]
        public long TimeTotalLoad { get; set; }

        [JsonProperty("time_waiting")]
        public long TimeWaiting { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("title_consistency")]
        public long TitleConsistency { get; set; }

        [JsonProperty("title_duplicate_tag")]
        public bool TitleDuplicateTag { get; set; }

        [JsonProperty("title_length")]
        public long TitleLength { get; set; }

        [JsonProperty("www")]
        public bool Www { get; set; }
    }

    public class LinksTo
    {
        [JsonProperty("alt")]
        public string Alt { get; set; }

        [JsonProperty("anchor")]
        public string Anchor { get; set; }

        [JsonProperty("link_from")]
        public Uri LinkFrom { get; set; }

        [JsonProperty("link_to")]
        public Uri LinkTo { get; set; }

        [JsonProperty("nofollow")]
        public bool Nofollow { get; set; }

        [JsonProperty("page_from")]
        public string PageFrom { get; set; }

        [JsonProperty("page_to")]
        public string PageTo { get; set; }

        [JsonProperty("relative")]
        public bool Relative { get; set; }

        [JsonProperty("ssl_from_use")]
        public bool SslFromUse { get; set; }

        [JsonProperty("ssl_to_use")]
        public bool SslToUse { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("text_post")]
        public string TextPost { get; set; }

        [JsonProperty("text_pre")]
        public string TextPre { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("www_from_use")]
        public bool WwwFromUse { get; set; }

        [JsonProperty("www_to_use")]
        public bool WwwToUse { get; set; }
    }









}
