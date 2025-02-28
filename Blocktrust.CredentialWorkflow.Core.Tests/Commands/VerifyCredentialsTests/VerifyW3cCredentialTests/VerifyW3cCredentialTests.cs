﻿using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckExpiry;
using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckRevocation;
using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.CheckSignature;
using Blocktrust.CredentialWorkflow.Core.Commands.VerifyCredentials.VerifyW3cCredentials.VerifyW3cCredential;
using Blocktrust.CredentialWorkflow.Core.Crypto;
using Blocktrust.CredentialWorkflow.Core.Services;
using Blocktrust.CredentialWorkflow.Core.Services.DIDPrism;
using FluentAssertions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Blocktrust.CredentialWorkflow.Core.Tests.Commands.VerifyCredentialsTests.VerifyW3cCredentialTests;

using Services;

public class VerifyW3CCredentialTests
{
    private readonly IMediator _mediator;
    private readonly CredentialParser _credentialParser;
    private readonly VerifyW3CCredentialHandler _handler;
    private readonly HttpClient _httpClient;

    private const string ValidJwt = """
                                    eyJhbGciOiJFUzI1NksifQ.eyJpc3MiOiJkaWQ6cHJpc206NTM5MDBiN2MxZjFjNzA0NGVkNDk4OWFiNDY1NzBkMjkyNzIzNmU5NDE0ZmViZTgyZWEzNmFhYTkxN2E2NDJkZDpDb1FCQ29FQkVrSUtEbTE1TFdsemMzVnBibWN0YTJWNUVBSktMZ29KYzJWamNESTFObXN4RWlFQ2ZkNmlDYnp2TENTT05lbG12czNvUzJJWXl1ZzhaM2hwOU1aZVMyVzJCcmtTT3dvSGJXRnpkR1Z5TUJBQlNpNEtDWE5sWTNBeU5UWnJNUkloQWt5WkVjQ0FhTC1WZFBuUU9PdHVsVjZEU0k2eGIxVVNXRXhvUWxJbmwybWEiLCJzdWIiOiJkaWQ6cHJpc206ZDIyNTBkOWVlMDYzYzNmNWJhZWQyMTJlZDQ1YzlmNTNjMDhiYzE4OWMwNDdhOTVkODZkYTUwZmZkZWY0M2NlZTpDbnNLZVJJNkNnWmhkWFJvTFRFUUJFb3VDZ2x6WldOd01qVTJhekVTSVFPZk5BZXR2QXVaQkZ6VzRWdGpCcy1MMlhDYnl2SGF5ZTJWa1NWcDlGNW9RQkk3Q2dkdFlYTjBaWEl3RUFGS0xnb0pjMlZqY0RJMU5tc3hFaUVDdmFXTnF2c3p2d25VUnVXeXhPVzNHbzJMcVA1ejdjS05WaldNNUxZNW4zOCIsIm5iZiI6MTcyNjg0MzE5NiwiZXhwIjoyMDI2ODQzMTk2LCJ2YyI6eyJjcmVkZW50aWFsU3ViamVjdCI6eyJhY2hpZXZlbWVudCI6eyJhY2hpZXZlbWVudFR5cGUiOiJEaXBsb21hIiwiaW1hZ2UiOnsiaWQiOiJcLzlqXC80QUFRU2taSlJnQUJBUUFBQVFBQkFBRFwvMndDRUFBa0dCd2dIQmdrSUJ3Z0tDZ2tMRFJZUERRd01EUnNVRlJBV0lCMGlJaUFkSHg4a0tEUXNKQ1l4Sng4ZkxUMHRNVFUzT2pvNkl5c1wvUkQ4NFF6UTVPamNCQ2dvS0RRd05HZzhQR2pjbEh5VTNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTlwvXC9BQUJFSUFKUUExZ01CSWdBQ0VRRURFUUhcL3hBQWJBQUVBQWdNQkFRQUFBQUFBQUFBQUFBQUFCZ2NCQkFVREF2XC9FQUVFUUFBRURBd0lEQlFVRUJ3WUhBQUFBQUFFQUFnTUVCUkVHSVFjU01STWlRVkZoRkRKeGdaRVZGa0toSTFKaWNxS3p3U1NDa3JHeThEWkVWWU9UMGRMXC94QUFZQVFFQUF3RUFBQUFBQUFBQUFBQUFBQUFBQVFJREJQXC9FQUNZUkFRQUNBUU1FQVFRREFBQUFBQUFBQUFBQkFoRURJVEVTUVdHaFVTSXlnZkFUSTNIXC8yZ0FNQXdFQUFoRURFUUFcL0FMeFJFUUVSRUJFUkFSRVFFUkVCRVJBUkVRRVJFQkVSQVJFUUVSRUJFUkFSRVFFUkVCRVJBUkVRRVJFQkVSQVJFUUZqSzhwNTRxYU4wczhySW8yN2w4amcxb0h4S2hWNDRvV1dsZjdOYUJOZDZ3N01qbzJGelNmM3ZINVpVMXJOdUVUYUk1VG9sYytHK1d5ZTRQdDhOZlRQcTJETG9XeUF1SHlWYVhTVFYxOHBuVk9wYnJCcFd6SHJEemhzcng2bjNzK21SOEZId3poek04VVZOVTNHaW1adkhkbk9kM25lWkhnUFhBV3NhWHl6blUrRjlnN0lxcW83bHJmVGNEWm8zeGFycytNc25weUhTdGJcL0FIZHpcL0Y4Vkk3SHhKMDlkWE5obHF2cytxSjVURFdqczlcL0lPNkVcL05WblRtT0ZvdkVwbWkrSTNpUm9jeHdjMDlDRGtGZmF6WEVSRUJFUkFSRVFFUkVCRVJBUkVRRVJFQkVSQVJFUWNEVitxS0hTdEhGVlhCazhnbWYyY1VjTGN1YzdCT1BMb0ZFVHF2VytvTXRzR25tVzJuUFNxcjNuT1BQQkF4K2E5ZU5lUlJhZGQ1WGVQXC9BRXVVSTR0M0s0SFZ0VlFPcjZvVWNjY1paVHRsTFk5MmduTFJzZFwvTmRHbHB4TVIrV043VERldkZGYVk1VE5ydlYwMTJuYnY3QlFIdWowUCsyclVqMXRVeE1OSm9uVHNWdWpjNFJDVU03U1Z4UFFFOUd1T0R0djhBRlErMFdxcHZGVzZqb25Sc2tFVDVBWkRodmRibGQrc3JLU2hpRWRuYUh3MWdZV1d5YnRXelU4d2FHOXJsdlZ4T2NEUGl0K21PSjNaUmFYQXZGVGNhcTRTbThUelMxYkhGcnUxZHpGaDhRUEFmSmR1NVZPbkg2UXBJS09rQXV6SEIwdUpua041dXBHZmVQZEhkSndPYnhVcjB0d25mVVFNcXRUVkVrYjVBSGV5d3VITUIrMjd6K0gxS2xqdUdXbERHWVwvWXBRUVBlRlM3bUhyMVZiYTFJMldycDJVdnBxdHZsTFg4bW5KcWdWTGdYZGpDN1orTnpscDJLa3NtdEtDN01aRHJYVGtWUUpHNUZiVERzcGNkT1lEeEd4M0JId1hSMVR3dXE3VTM3UTAxTkxWTmlQTzZuZTdFZ0E2OHJoMTI4Tmo1RlI4T3BMN1NPTHl6dG5NRkpRVytuNVwvN0E0dUI1M0YzNER2bnJqSkhWV3pTKzhLNHRYWkk3SlFGaEVuRFwvQUZweXQ2aTNYQTRQd0dkdjRmbXU3OSt0VDJJOG1xdE1TU1F0OTZyb0haSCtIcDg4aFUxY2FLUzNYQ3BvNXkxMHRQSTVqaXc3WkcyUVZPdUdsMHVOUlRhaG82aXZxWjZXSzJ2ZXlLYVF2RERuRzJlbTNnTmxXK25HTThwcmVjNFhIcHU5VW1vYlZGYzZBdjhBWjVTNE5EMjRjQzF4YVFmbUN1b29Sd1lHT0hkdFBtK2YrYzlUZGNsNHhhWWgweHdJaUtxUkVSQVJFUUVSRUJFUkFSRVFFUkVCRVJCWEhHM2EwV04zbGRvdjlEMVhcL0Z3WTExVmVaZ2hQOEtzRGpsdHArMU9cL1Z1Y1pcL2dlb0Z4Z1wvNDNuOWFhRVwva1YyYUhiOHVmVjd0S3l3UTAxcmdqdTlGV3dOcjUyelVWenBHYzhqUTBZZXdBWnowR3g4enRzcEpvRVVsWmY3bHF1XC8xampUVUx1U21tcm5ORHVZNXdYWUFITUc3QUFkU2R0bHE2WnFLYWxwTFkyaTFUVVVzTDRaRE5SdGlENW81ajFiRnQ0a2tqUDYzN1MzZUhsTGI3aG91OFVkMHBSTVk2NXZNMTdNdllYRm96NWc5UVQ4Vk41MmxGWTNoNzZyNHRudFBZOU1SdHk3YjJ1ZHVTZjNHZVB4UDBVRml1R3FhZTR1dXpYWGR0VWZlcUhReVlJOWNqR1BUb3BKWFdyVTFIZUsrMDZTWlVSVU5QTVdCMEhKRzkyMmU4ODRjN3I1cnhHaytJelhkb0RkTWpmUDJwdjhBekZOT2lzYkl0MVRPNlI2VDR1UVZCYlM2bWpaRklOaFZ3TlBJZjNtXC9oUG1SdDhGek5XVTFKYTlXeDExc3JhcUsxWDJGM2F1dGpXeU9jN3h3M0J6dVFTQU05VjUyT3pYdTc2Z3ByVHJPS2FTbWN4MGdmSVdHWExjWUFrSGV3ZlVyc2NRNldodGx5MHJiN1pFK2piRk8rVWV3eFwvcEkyN1pjQmpjN0U3NTZLbUlpXC93QksyODF6S0FYNkp0UmJhU3J0MXVtaG9LWE5OSldTN09xWmk0a3VQbWR2a2Nqd1haNFk3UTZwZityYWpcL21mXC9TODlYVFU5VGJLbVNvMUlibE8ydFBzbFBFd05hQVFPWjdcL1hjK21cLzdTOU9HMjF0MWNmSzFcLzhBMHRKbit0U0l4ZFp2QnNZNGMycjk2b1wvbnlLYUtHOElCamgzYWYrOVwvT2Vwa3VQVSsrWFRUN1lFUkZSWVJFUUVSRUJFUkFSRVFFUkVCRVJBUllkMFVPMUh4RHRXbTdrNmd1ZE5YaVVNRHc2T0VPYThIeEI1dlFoVEVUUENKbUk1YVhHU2dyTG5wMmppdDFKUFZTc3JtUExJWXk0Z0JydDl2RGRRdmlwWWJ4WDZ2ZFUwTnJyYWlFMDBUZTBpZ2M1dVJuSXlBdXRyVzRYTzk2cHNGSlk3clUyNks1MGJYczd4QUJMbkhKQVBYQXdvdXo3ME9iWjNEVXRVUHRTc2RTc1wvU3ZcL1JscitUSjMzQzZ0T0ppSTNZWDNtV3haNHRRV3VocDNVZWhwRGRxTnhkRlhleWNybnQ4bmRNbjZcLzFYUmlzZHcwXC9xWXd3VVYycXJWWHhRKzN6eFFPTHUxRCtjdkczNjdjNDhubjBYS3ZzT3E3SEJVVFZXbzZxUVExemFRY2tyK1wvelJoNGVOK20rTWVpeFdSYW9vNnlcLzBoMU5WU1NXV25FOGdiSzhkczNBSjVkOXNBaFdtTVwvQ0kyN0x3cUtDanJUelZWSkZQa2JkcEVDZnpXdjhBZCswZjlMcFBcL0VGVGNWTnFaMTVmYkpOVjFNTDRxQVZzMHBlOHRqYnk4M0xqT1NRTUZhMXFxYjFlTHBOUjBHczU1S2VLamZWT3JNU2h2SzBnT0hLZTluY0xMK0x5dkdwNFhwRlIwbHZoZStucEk0bWhwSkVNVzVId0hWVmZQSGZyMWZicnFSOXN1TkpWVUxHR3p3U1U3Z0g0THNodzgzQUg0Y3k0aktUVlVtb1lMUEZxZWQ0bm9oV3NxUkxKeW1JNTM1ZmV6c2RsNVVadjlkZWFlM1VPc0phbGxSVE9xSTZsa2p3TU5hWFljek9XdTJ4aFdycDQzeWliNTdQbStXMjhWdEpGQkZvbVdscWk4eTFOWEhTbHo1WEVrNERobmJmeitRWFQwSFlieFIyalZiYXExMXNMNmkzQmtEWklYTk1qdVwvc050ejBYTnNzdDV1ZG1xTG83VzFSU3NwY2UwUlBiSzkwZk03RGR3Y0hQb3ZxMHhhcHVsRmI2dURVMVUyQ3JmTEhLWFN2XC9BTEtZMjg1NXQ5OGdFXC9KYVRuR0ZJNXl0amhqUzFGQm9lMTB0WkJKVHpzYkp6eFN0TFhOekk0N2dcL0ZTcFZEdzUxWSszYWV2VjB2bFpXVjBGUFVSc2E0bm5jQWNqWUUrYW0ybGRhMEdxcDZpTzEwOVlHMDdRWkpKb2cxb3lkaG5KMzJKK1M1ZFNsdXFaYjBtTVJDVUlzRG9GbFpyaUlpQWlJZ0lpSUNJaUFpSWdJaUlNSG9xNzQwV1duck5OT3VaTEk2aWhJTFhFYnZhNGhwYitlUjhGWWg2S0I4VnJWZXI5YTZhM1dhbEVzWm01NXlYaHV6UnNOXC9WWDA5cnhLdDQrbVVIdkYxaHN0KzBYY3FsaGZGVDJ5TXVEZXVNdUczMVhPcWRSMmVDcTA3RlNUenpVOXRyM1ZVMHo0K1FrT2w1OEJ1ZW9Da3I0dGFVVnJpRmJwbXpWRU5GQUdDU1p2TzRNYlwvZStLK0x0cUJzV2o3UGY2Q3hXYVJ0Um1Lc2ErbHlJcFI1WVBUSVA1THBqR3pEZjVSM1V1cnFPODZhaG9jeWUxeFhKMDNNVzdPaEJkeUhPZW9EZ01laSsyNnF0UjRpWEM3eUdVMm12Z2ZUemQzdjhqbzJqM2NcL3JOSHlYMk5aWEEwbnRZMHZZdlo4T1BQN0x0aHZ2Zml6c3ZXWFU5NGhZOTh1azdHeGpHOHpuR21HQVA4ZlhZN2RkbGJwMnhqMnJudm4wMXJmcXloaTF0ZGJ2TFBORFRWTlBKRFR5TWpEbnN5ME5ZY2VtTXJYczJvNlMxNm11RnlrdU5UV21lM1N4TnFud05hOTBydVRHV2RNRGxXKzdWVjJaSExMSnBPeXRqaEFNcmpTYk1CYnpEUGU4UnV2b2FudTdobHVrN0lmMFlreDdMdnlrWkczTm5KOHVxbkhqMlpcL2NOQ3QxRmE3anFpZ3VsVFVWc0hMYm80NXA2YnVTTXFXODNmQUhWdTQyR01yWkdxTEpGclNudThZZnlOb254VlU3SVF3MUV6b3kzbjVBZHNuR1Y2MU9xcnJTeHVrcU5LMktOalMxcmlhWWJGM1FiUDhBRmVuM2l2ZWNmZEd5QThcL1pnR2xBeTdPTWJ2OEFOUjArUFpuejZSbXpYZWtvdEwzeTNUdWQyMWFJdXl3M2J1dXljbk95OWJGcUJsdjB0ZjdXNlo0ZlhzajdBTkdSbkpEeVQ0WmJnTHRWZXJybFJOYStyMHZZb211SkRTNm02a0VnOUhlWVAwWGNudnBwdEUwVjFxTEJhUHRHNVZnaXBJRzAzZExOOHVJem5vUFB4Q20wNDVqbnlSR2VcL3BGcks3UERUVVp6XC93QTNUNXdyajRkMlNuc21tS1JrQlkrU2Rvbm1sYitOemhuOHVpZ3QydFd0N25hSnJhTk8ycWxwNTNOYzgwMkdPUEtjajhTbmZEeW11bHYwelRVRjZwK3lucHlZMjRjSFpZRDNkeDZMRFduTVR2M2E2Y2JwT09peWc2SXVkc0lpSUNJaUFpSWdJaUlDSWlBaUlnRll3c29nK0h0RGdXdUFMU01FSHhDcDJhaGcwdmVhXC9URjdhZnUxZTNGMUxPVHRUeVp5M2Z3TFRnWjlHbnp4Y25pdVRxV3dVT283WExiN2pIelJ1M2E0ZTh4dzZFSFwvQUhsWHBicFwveFcwWlVEZmFXN2FScUpMUFVSeFBwbmN6bVBmRUhNcVdPMnpueDhzZFI4TUw1WmQzbHZNNitRdGM5cDUyXC9acmozem5tY2NEZHg1blpQamtxWVZcL3RPbVlmdVwvcnFsZGN0UHlPeFNYS01FdmdQZ00rQjlQOEFQb0k1ZTlCMWRQU0M1YWZuRjV0YnQyeTA0ekl3ZnROSDlQb0YyVnRXZVhOTVREVG11ejU0WllwZFJ0YzJhUHM1QjludkhPM2ZBT0I0WitXQWh1bVdNQnZzVHVSZ2EwbTJ2eUNCZ1A2ZThCNCtwWERvcElZNWlhaU5zakN4N1Mxd0p3ZVU0STlRY0xzbXVzaHJTNFVVZllGelNlNzA3c21mRG9DWThkM3dQeFduVGpoVm1xdUxLcGtyWmI1Qmlia01tTGRKbDNKMFgxVTM2cTdOMGpiNktpWnNnbFlEU1NOZHpaemtPT3crYTREbTl0VkZsS3h6akpJUkRFMEV1eG5ab0E2bkNtZHQwTEhRVWpMcHJXckZzb3M1YlRBXC9wNXYyY2VIeTMrQ2kzVFhrak10ZlRscnI5WXl0TnpsYkJhS0l1a3FheDNjRFFTWEVBNTZra24wK2ltZWw0ZnZycTJLN3gwNWcwN1ptOWxRTUxlVVNPSFREVDlmOEk2NVdyUVVOdzE3SERTVytsTmwwZEFSeU5hTVBxY2VQcm42RHpKNldyYkxmUzJ5aWhvNktKc1VFVFExakcrQVwvcWZWYzJycU42VmJRSG1zNFdVWE0yRVJFQkVSQVJFUUVSRUJFUkFSRVFFUkVCRVJBUkVRZU5WVFFWVk8rbnFZWTVvWGpsZEc5b0xYRHlJVmVYSGg1VzJpcWZjZEIzSjl2bk83cVNWeE1NbnA0XC9uOVFySlhtWCtqdm9yVnROVVRFU3BXOVYxdW5tTVhFVFRVMXRyRHQ5cDBUTm5lcDVldnk1bG9PMGZwbWtiOW9WMnNLZDlwUHVkaXpNNzhmaHdNN1wvQVorQ3ZHcmlncTRYUTFOT0pZbkRCWkl3T0IrU2k5Tnc3MHJUWEoxZkhhUVhuZHNUM09kRTArWWFUaitua3RxNjBZN3NwMDVtVU1zdGJVUGFZT0cybURDQ09VM1d1YU9Zanp5ZkQwejhsSnJKdzNpTldMbnF5c2t2VnhQVVNrOWszMEFQVWVtQVBSVG1Ia2pqREk0aXhqZGcxcmNBTDFEXC9SMzBWSjFabmhhS1k1Wll4ckdocldoclIwQUdBRjlJaXlhQ0lpQWlJZ0lpSUNJaUFpSWdJaUlDSWlBaUlnSWlJQ0lpQWlJZ3dtNnlpREN5aUlDSWlBaUlnSWlJQ0lpQWlJZ0lpSUNJaUFpSWdJaUlDSWlBaUlnSWlJQ0lpQWlJZ0lpSUNJaUFpSWdJaUlDSWlBaUlnXC9cL1oiLCJ0eXBlIjoiSW1hZ2UifSwiY3JpdGVyaWEiOnsibmFycmF0aXZlIjoiVGhyb3VnaCBzZXJpZXMgb2YgbGVzc29ucy5BbGlxdWFtIGVyYXQgdm9sdXRwYXQuIERvbmVjIGltcGVyZGlldCBlcm9zIHNhcGllbiwgZWdldCBwaGFyZXRyYSBzYXBpZW4gdnVscHV0YXRlIHV0LiBEdWlzIGlkIHZvbHV0cGF0IGRvbG9yLiBQcm9pbiB0aW5jaWR1bnQgbWF4aW11cyBibGFuZGl0LiBOYW0gaWQgbWFsZXN1YWRhIGVyYXQsIHNpdCBhbWV0IHNvZGFsZXMgbGVjdHVzLiBGdXNjZSB1dCBjb25zZXF1YXQgcHVydXMuIFV0IGludGVyZHVtIHNhcGllbiBldCB0b3J0b3IgbGFvcmVldCBwdWx2aW5hci4iLCJ0eXBlIjoiQ3JpdGVyaWEifSwibmFtZSI6IkRpZ2l0YWwgSWRlbnRpdHkgQ291cnNlIiwiZGVzY3JpcHRpb24iOiJBIGNvdXJzZSBvbiBEaWdpdGFsIGlkZW50aXR5IiwiaWQiOiJ1cm46dXVpZDphMzVhOGU0Zi03Y2Q0LTQwZGEtYWYzMi00MDI4OWRjMTZkOTEiLCJ0eXBlIjpbIkFjaGlldmVtZW50Il19LCJpZCI6ImRpZDpwcmlzbTpkMjI1MGQ5ZWUwNjNjM2Y1YmFlZDIxMmVkNDVjOWY1M2MwOGJjMTg5YzA0N2E5NWQ4NmRhNTBmZmRlZjQzY2VlOkNuc0tlUkk2Q2daaGRYUm9MVEVRQkVvdUNnbHpaV053TWpVMmF6RVNJUU9mTkFldHZBdVpCRnpXNFZ0akJzLUwyWENieXZIYXllMlZrU1ZwOUY1b1FCSTdDZ2R0WVhOMFpYSXdFQUZLTGdvSmMyVmpjREkxTm1zeEVpRUN2YVdOcXZzenZ3blVSdVd5eE9XM0dvMkxxUDV6N2NLTlZqV001TFk1bjM4IiwidHlwZSI6WyJBY2hpZXZlbWVudFN1YmplY3QiXX0sInR5cGUiOlsiVmVyaWZpYWJsZUNyZWRlbnRpYWwiXSwiQGNvbnRleHQiOlsiaHR0cHM6XC9cL3d3dy53My5vcmdcLzIwMThcL2NyZWRlbnRpYWxzXC92MSJdLCJjcmVkZW50aWFsU3RhdHVzIjp7InN0YXR1c1B1cnBvc2UiOiJSZXZvY2F0aW9uIiwic3RhdHVzTGlzdEluZGV4Ijo1LCJpZCI6Imh0dHA6XC9cLzEwLjEwLjUwLjEwNTo4MDAwXC9jbG91ZC1hZ2VudFwvY3JlZGVudGlhbC1zdGF0dXNcL2I5YjZiYjFlLTY4NjQtNDA3NC1iOGFjLTEyYjNhMGIzMGYwYyM1IiwidHlwZSI6IlN0YXR1c0xpc3QyMDIxRW50cnkiLCJzdGF0dXNMaXN0Q3JlZGVudGlhbCI6Imh0dHA6XC9cLzEwLjEwLjUwLjEwNTo4MDAwXC9jbG91ZC1hZ2VudFwvY3JlZGVudGlhbC1zdGF0dXNcL2I5YjZiYjFlLTY4NjQtNDA3NC1iOGFjLTEyYjNhMGIzMGYwYyJ9fX0.RaXJtH8DXmHEEDpJMrbT8e-AxsjH4zqXeGHIxT0Ly13zzJ0GStlFkdw_SLiggQI_7iyl9xupf2icaPURcOkouw
                                    """;

    private const string ExpiredJwt = """
                                      eyJhbGciOiJFUzI1NksifQ.eyJpc3MiOiJkaWQ6cHJpc206NTM5MDBiN2MxZjFjNzA0NGVkNDk4OWFiNDY1NzBkMjkyNzIzNmU5NDE0ZmViZTgyZWEzNmFhYTkxN2E2NDJkZDpDb1FCQ29FQkVrSUtEbTE1TFdsemMzVnBibWN0YTJWNUVBSktMZ29KYzJWamNESTFObXN4RWlFQ2ZkNmlDYnp2TENTT05lbG12czNvUzJJWXl1ZzhaM2hwOU1aZVMyVzJCcmtTT3dvSGJXRnpkR1Z5TUJBQlNpNEtDWE5sWTNBeU5UWnJNUkloQWt5WkVjQ0FhTC1WZFBuUU9PdHVsVjZEU0k2eGIxVVNXRXhvUWxJbmwybWEiLCJzdWIiOiJkaWQ6cHJpc206ZDIyNTBkOWVlMDYzYzNmNWJhZWQyMTJlZDQ1YzlmNTNjMDhiYzE4OWMwNDdhOTVkODZkYTUwZmZkZWY0M2NlZTpDbnNLZVJJNkNnWmhkWFJvTFRFUUJFb3VDZ2x6WldOd01qVTJhekVTSVFPZk5BZXR2QXVaQkZ6VzRWdGpCcy1MMlhDYnl2SGF5ZTJWa1NWcDlGNW9RQkk3Q2dkdFlYTjBaWEl3RUFGS0xnb0pjMlZqY0RJMU5tc3hFaUVDdmFXTnF2c3p2d25VUnVXeXhPVzNHbzJMcVA1ejdjS05WaldNNUxZNW4zOCIsIm5iZiI6MTcyNjg0MzE5NiwiZXhwIjoyMDI2ODQzMTk2LCJ2YyI6eyJjcmVkZW50aWFsU3ViamVjdCI6eyJhY2hpZXZlbWVudCI6eyJhY2hpZXZlbWVudFR5cGUiOiJEaXBsb21hIiwiaW1hZ2UiOnsiaWQiOiJcLzlqXC80QUFRU2taSlJnQUJBUUFBQVFBQkFBRFwvMndDRUFBa0dCd2dIQmdrSUJ3Z0tDZ2tMRFJZUERRd01EUnNVRlJBV0lCMGlJaUFkSHg4a0tEUXNKQ1l4Sng4ZkxUMHRNVFUzT2pvNkl5c1wvUkQ4NFF6UTVPamNCQ2dvS0RRd05HZzhQR2pjbEh5VTNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTlwvXC9BQUJFSUFKUUExZ01CSWdBQ0VRRURFUUhcL3hBQWJBQUVBQWdNQkFRQUFBQUFBQUFBQUFBQUFCZ2NCQkFVREF2XC9FQUVFUUFBRURBd0lEQlFVRUJ3WUhBQUFBQUFFQUFnTUVCUkVHSVFjU01STWlRVkZoRkRKeGdaRVZGa0toSTFKaWNxS3p3U1NDa3JHeThEWkVWWU9UMGRMXC94QUFZQVFFQUF3RUFBQUFBQUFBQUFBQUFBQUFBQVFJREJQXC9FQUNZUkFRQUNBUU1FQVFRREFBQUFBQUFBQUFBQkFoRURJVEVTUVdHaFVTSXlnZkFUSTNIXC8yZ0FNQXdFQUFoRURFUUFcL0FMeFJFUUVSRUJFUkFSRVFFUkVCRVJBUkVRRVJFQkVSQVJFUUVSRUJFUkFSRVFFUkVCRVJBUkVRRVJFQkVSQVJFUUZqSzhwNTRxYU4wczhySW8yN2w4amcxb0h4S2hWNDRvV1dsZjdOYUJOZDZ3N01qbzJGelNmM3ZINVpVMXJOdUVUYUk1VG9sYytHK1d5ZTRQdDhOZlRQcTJETG9XeUF1SHlWYVhTVFYxOHBuVk9wYnJCcFd6SHJEemhzcng2bjNzK21SOEZId3poek04VVZOVTNHaW1adkhkbk9kM25lWkhnUFhBV3NhWHl6blUrRjlnN0lxcW83bHJmVGNEWm8zeGFycytNc25weUhTdGJcL0FIZHpcL0Y4Vkk3SHhKMDlkWE5obHF2cytxSjVURFdqczlcL0lPNkVcL05WblRtT0ZvdkVwbWkrSTNpUm9jeHdjMDlDRGtGZmF6WEVSRUJFUkFSRVFFUkVCRVJBUkVRRVJFQkVSQVJFUWNEVitxS0hTdEhGVlhCazhnbWYyY1VjTGN1YzdCT1BMb0ZFVHF2VytvTXRzR25tVzJuUFNxcjNuT1BQQkF4K2E5ZU5lUlJhZGQ1WGVQXC9BRXVVSTR0M0s0SFZ0VlFPcjZvVWNjY1paVHRsTFk5MmduTFJzZFwvTmRHbHB4TVIrV043VERldkZGYVk1VE5ydlYwMTJuYnY3QlFIdWowUCsyclVqMXRVeE1OSm9uVHNWdWpjNFJDVU03U1Z4UFFFOUd1T0R0djhBRlErMFdxcHZGVzZqb25Sc2tFVDVBWkRodmRibGQrc3JLU2hpRWRuYUh3MWdZV1d5YnRXelU4d2FHOXJsdlZ4T2NEUGl0K21PSjNaUmFYQXZGVGNhcTRTbThUelMxYkhGcnUxZHpGaDhRUEFmSmR1NVZPbkg2UXBJS09rQXV6SEIwdUpua041dXBHZmVQZEhkSndPYnhVcjB0d25mVVFNcXRUVkVrYjVBSGV5d3VITUIrMjd6K0gxS2xqdUdXbERHWVwvWXBRUVBlRlM3bUhyMVZiYTFJMldycDJVdnBxdHZsTFg4bW5KcWdWTGdYZGpDN1orTnpscDJLa3NtdEtDN01aRHJYVGtWUUpHNUZiVERzcGNkT1lEeEd4M0JId1hSMVR3dXE3VTM3UTAxTkxWTmlQTzZuZTdFZ0E2OHJoMTI4Tmo1RlI4T3BMN1NPTHl6dG5NRkpRVytuNVwvN0E0dUI1M0YzNER2bnJqSkhWV3pTKzhLNHRYWkk3SlFGaEVuRFwvQUZweXQ2aTNYQTRQd0dkdjRmbXU3OSt0VDJJOG1xdE1TU1F0OTZyb0haSCtIcDg4aFUxY2FLUzNYQ3BvNXkxMHRQSTVqaXc3WkcyUVZPdUdsMHVOUlRhaG82aXZxWjZXSzJ2ZXlLYVF2RERuRzJlbTNnTmxXK25HTThwcmVjNFhIcHU5VW1vYlZGYzZBdjhBWjVTNE5EMjRjQzF4YVFmbUN1b29Sd1lHT0hkdFBtK2YrYzlUZGNsNHhhWWgweHdJaUtxUkVSQVJFUUVSRUJFUkFSRVFFUkVCRVJCWEhHM2EwV04zbGRvdjlEMVhcL0Z3WTExVmVaZ2hQOEtzRGpsdHArMU9cL1Z1Y1pcL2dlb0Z4Z1wvNDNuOWFhRVwva1YyYUhiOHVmVjd0S3l3UTAxcmdqdTlGV3dOcjUyelVWenBHYzhqUTBZZXdBWnowR3g4enRzcEpvRVVsWmY3bHF1XC8xampUVUx1U21tcm5ORHVZNXdYWUFITUc3QUFkU2R0bHE2WnFLYWxwTFkyaTFUVVVzTDRaRE5SdGlENW81ajFiRnQ0a2tqUDYzN1MzZUhsTGI3aG91OFVkMHBSTVk2NXZNMTdNdllYRm96NWc5UVQ4Vk41MmxGWTNoNzZyNHRudFBZOU1SdHk3YjJ1ZHVTZjNHZVB4UDBVRml1R3FhZTR1dXpYWGR0VWZlcUhReVlJOWNqR1BUb3BKWFdyVTFIZUsrMDZTWlVSVU5QTVdCMEhKRzkyMmU4ODRjN3I1cnhHaytJelhkb0RkTWpmUDJwdjhBekZOT2lzYkl0MVRPNlI2VDR1UVZCYlM2bWpaRklOaFZ3TlBJZjNtXC9oUG1SdDhGek5XVTFKYTlXeDExc3JhcUsxWDJGM2F1dGpXeU9jN3h3M0J6dVFTQU05VjUyT3pYdTc2Z3ByVHJPS2FTbWN4MGdmSVdHWExjWUFrSGV3ZlVyc2NRNldodGx5MHJiN1pFK2piRk8rVWV3eFwvcEkyN1pjQmpjN0U3NTZLbUlpXC93QksyODF6S0FYNkp0UmJhU3J0MXVtaG9LWE5OSldTN09xWmk0a3VQbWR2a2Nqd1haNFk3UTZwZityYWpcL21mXC9TODlYVFU5VGJLbVNvMUlibE8ydFBzbFBFd05hQVFPWjdcL1hjK21cLzdTOU9HMjF0MWNmSzFcLzhBMHRKbit0U0l4ZFp2QnNZNGMycjk2b1wvbnlLYUtHOElCamgzYWYrOVwvT2Vwa3VQVSsrWFRUN1lFUkZSWVJFUUVSRUJFUkFSRVFFUkVCRVJBUllkMFVPMUh4RHRXbTdrNmd1ZE5YaVVNRHc2T0VPYThIeEI1dlFoVEVUUENKbUk1YVhHU2dyTG5wMmppdDFKUFZTc3JtUExJWXk0Z0JydDl2RGRRdmlwWWJ4WDZ2ZFUwTnJyYWlFMDBUZTBpZ2M1dVJuSXlBdXRyVzRYTzk2cHNGSlk3clUyNks1MGJYczd4QUJMbkhKQVBYQXdvdXo3ME9iWjNEVXRVUHRTc2RTc1wvU3ZcL1JscitUSjMzQzZ0T0ppSTNZWDNtV3haNHRRV3VocDNVZWhwRGRxTnhkRlhleWNybnQ4bmRNbjZcLzFYUmlzZHcwXC9xWXd3VVYycXJWWHhRKzN6eFFPTHUxRCtjdkczNjdjNDhubjBYS3ZzT3E3SEJVVFZXbzZxUVExemFRY2tyK1wvelJoNGVOK20rTWVpeFdSYW9vNnlcLzBoMU5WU1NXV25FOGdiSzhkczNBSjVkOXNBaFdtTVwvQ0kyN0x3cUtDanJUelZWSkZQa2JkcEVDZnpXdjhBZCswZjlMcFBcL0VGVGNWTnFaMTVmYkpOVjFNTDRxQVZzMHBlOHRqYnk4M0xqT1NRTUZhMXFxYjFlTHBOUjBHczU1S2VLamZWT3JNU2h2SzBnT0hLZTluY0xMK0x5dkdwNFhwRlIwbHZoZStucEk0bWhwSkVNVzVId0hWVmZQSGZyMWZicnFSOXN1TkpWVUxHR3p3U1U3Z0g0THNodzgzQUg0Y3k0aktUVlVtb1lMUEZxZWQ0bm9oV3NxUkxKeW1JNTM1ZmV6c2RsNVVadjlkZWFlM1VPc0phbGxSVE9xSTZsa2p3TU5hWFljek9XdTJ4aFdycDQzeWliNTdQbStXMjhWdEpGQkZvbVdscWk4eTFOWEhTbHo1WEVrNERobmJmeitRWFQwSFlieFIyalZiYXExMXNMNmkzQmtEWklYTk1qdVwvc050ejBYTnNzdDV1ZG1xTG83VzFSU3NwY2UwUlBiSzkwZk03RGR3Y0hQb3ZxMHhhcHVsRmI2dURVMVUyQ3JmTEhLWFN2XC9BTEtZMjg1NXQ5OGdFXC9KYVRuR0ZJNXl0amhqUzFGQm9lMTB0WkJKVHpzYkp6eFN0TFhOekk0N2dcL0ZTcFZEdzUxWSszYWV2VjB2bFpXVjBGUFVSc2E0bm5jQWNqWUUrYW0ybGRhMEdxcDZpTzEwOVlHMDdRWkpKb2cxb3lkaG5KMzJKK1M1ZFNsdXFaYjBtTVJDVUlzRG9GbFpyaUlpQWlJZ0lpSUNJaUFpSWdJaUlNSG9xNzQwV1duck5OT3VaTEk2aWhJTFhFYnZhNGhwYitlUjhGWWg2S0I4VnJWZXI5YTZhM1dhbEVzWm01NXlYaHV6UnNOXC9WWDA5cnhLdDQrbVVIdkYxaHN0KzBYY3FsaGZGVDJ5TXVEZXVNdUczMVhPcWRSMmVDcTA3RlNUenpVOXRyM1ZVMHo0K1FrT2w1OEJ1ZW9Da3I0dGFVVnJpRmJwbXpWRU5GQUdDU1p2TzRNYlwvZStLK0x0cUJzV2o3UGY2Q3hXYVJ0Um1Lc2ErbHlJcFI1WVBUSVA1THBqR3pEZjVSM1V1cnFPODZhaG9jeWUxeFhKMDNNVzdPaEJkeUhPZW9EZ01laSsyNnF0UjRpWEM3eUdVMm12Z2ZUemQzdjhqbzJqM2NcL3JOSHlYMk5aWEEwbnRZMHZZdlo4T1BQN0x0aHZ2Zml6c3ZXWFU5NGhZOTh1azdHeGpHOHpuR21HQVA4ZlhZN2RkbGJwMnhqMnJudm4wMXJmcXloaTF0ZGJ2TFBORFRWTlBKRFR5TWpEbnN5ME5ZY2VtTXJYczJvNlMxNm11RnlrdU5UV21lM1N4TnFud05hOTBydVRHV2RNRGxXKzdWVjJaSExMSnBPeXRqaEFNcmpTYk1CYnpEUGU4UnV2b2FudTdobHVrN0lmMFlreDdMdnlrWkczTm5KOHVxbkhqMlpcL2NOQ3QxRmE3anFpZ3VsVFVWc0hMYm80NXA2YnVTTXFXODNmQUhWdTQyR01yWkdxTEpGclNudThZZnlOb254VlU3SVF3MUV6b3kzbjVBZHNuR1Y2MU9xcnJTeHVrcU5LMktOalMxcmlhWWJGM1FiUDhBRmVuM2l2ZWNmZEd5QThcL1pnR2xBeTdPTWJ2OEFOUjArUFpuejZSbXpYZWtvdEwzeTNUdWQyMWFJdXl3M2J1dXljbk95OWJGcUJsdjB0ZjdXNlo0ZlhzajdBTkdSbkpEeVQ0WmJnTHRWZXJybFJOYStyMHZZb211SkRTNm02a0VnOUhlWVAwWGNudnBwdEUwVjFxTEJhUHRHNVZnaXBJRzAzZExOOHVJem5vUFB4Q20wNDVqbnlSR2VcL3BGcks3UERUVVp6XC93QTNUNXdyajRkMlNuc21tS1JrQlkrU2Rvbm1sYitOemhuOHVpZ3QydFd0N25hSnJhTk8ycWxwNTNOYzgwMkdPUEtjajhTbmZEeW11bHYwelRVRjZwK3lucHlZMjRjSFpZRDNkeDZMRFduTVR2M2E2Y2JwT09peWc2SXVkc0lpSUNJaUFpSWdJaUlDSWlBaUlnRll3c29nK0h0RGdXdUFMU01FSHhDcDJhaGcwdmVhXC9URjdhZnUxZTNGMUxPVHRUeVp5M2Z3TFRnWjlHbnp4Y25pdVRxV3dVT283WExiN2pIelJ1M2E0ZTh4dzZFSFwvQUhsWHBicFwveFcwWlVEZmFXN2FScUpMUFVSeFBwbmN6bVBmRUhNcVdPMnpueDhzZFI4TUw1WmQzbHZNNitRdGM5cDUyXC9acmozem5tY2NEZHg1blpQamtxWVZcL3RPbVlmdVwvcnFsZGN0UHlPeFNYS01FdmdQZ00rQjlQOEFQb0k1ZTlCMWRQU0M1YWZuRjV0YnQyeTA0ekl3ZnROSDlQb0YyVnRXZVhOTVREVG11ejU0WllwZFJ0YzJhUHM1QjludkhPM2ZBT0I0WitXQWh1bVdNQnZzVHVSZ2EwbTJ2eUNCZ1A2ZThCNCtwWERvcElZNWlhaU5zakN4N1Mxd0p3ZVU0STlRY0xzbXVzaHJTNFVVZllGelNlNzA3c21mRG9DWThkM3dQeFduVGpoVm1xdUxLcGtyWmI1Qmlia01tTGRKbDNKMFgxVTM2cTdOMGpiNktpWnNnbFlEU1NOZHpaemtPT3crYTREbTl0VkZsS3h6akpJUkRFMEV1eG5ab0E2bkNtZHQwTEhRVWpMcHJXckZzb3M1YlRBXC9wNXYyY2VIeTMrQ2kzVFhrak10ZlRscnI5WXl0TnpsYkJhS0l1a3FheDNjRFFTWEVBNTZra24wK2ltZWw0ZnZycTJLN3gwNWcwN1ptOWxRTUxlVVNPSFREVDlmOEk2NVdyUVVOdzE3SERTVytsTmwwZEFSeU5hTVBxY2VQcm42RHpKNldyYkxmUzJ5aWhvNktKc1VFVFExakcrQVwvcWZWYzJycU42VmJRSG1zNFdVWE0yRVJFQkVSQVJFUUVSRUJFUkFSRVFFUkVCRVJBUkVRZU5WVFFWVk8rbnFZWTVvWGpsZEc5b0xYRHlJVmVYSGg1VzJpcWZjZEIzSjl2bk83cVNWeE1NbnA0XC9uOVFySlhtWCtqdm9yVnROVVRFU3BXOVYxdW5tTVhFVFRVMXRyRHQ5cDBUTm5lcDVldnk1bG9PMGZwbWtiOW9WMnNLZDlwUHVkaXpNNzhmaHdNN1wvQVorQ3ZHcmlncTRYUTFOT0pZbkRCWkl3T0IrU2k5Tnc3MHJUWEoxZkhhUVhuZHNUM09kRTArWWFUaitua3RxNjBZN3NwMDVtVU1zdGJVUGFZT0cybURDQ09VM1d1YU9Zanp5ZkQwejhsSnJKdzNpTldMbnF5c2t2VnhQVVNrOWszMEFQVWVtQVBSVG1Ia2pqREk0aXhqZGcxcmNBTDFEXC9SMzBWSjFabmhhS1k1Wll4ckdocldoclIwQUdBRjlJaXlhQ0lpQWlJZ0lpSUNJaUFpSWdJaUlDSWlBaUlnSWlJQ0lpQWlJZ3dtNnlpREN5aUlDSWlBaUlnSWlJQ0lpQWlJZ0lpSUNJaUFpSWdJaUlDSWlBaUlnSWlJQ0lpQWlJZ0lpSUNJaUFpSWdJaUlDSWlBaUlnXC9cL1oiLCJ0eXBlIjoiSW1hZ2UifSwiY3JpdGVyaWEiOnsibmFycmF0aXZlIjoiVGhyb3VnaCBzZXJpZXMgb2YgbGVzc29ucy5BbGlxdWFtIGVyYXQgdm9sdXRwYXQuIERvbmVjIGltcGVyZGlldCBlcm9zIHNhcGllbiwgZWdldCBwaGFyZXRyYSBzYXBpZW4gdnVscHV0YXRlIHV0LiBEdWlzIGlkIHZvbHV0cGF0IGRvbG9yLiBQcm9pbiB0aW5jaWR1bnQgbWF4aW11cyBibGFuZGl0LiBOYW0gaWQgbWFsZXN1YWRhIGVyYXQsIHNpdCBhbWV0IHNvZGFsZXMgbGVjdHVzLiBGdXNjZSB1dCBjb25zZXF1YXQgcHVydXMuIFV0IGludGVyZHVtIHNhcGllbiBldCB0b3J0b3IgbGFvcmVldCBwdWx2aW5hci4iLCJ0eXBlIjoiQ3JpdGVyaWEifSwibmFtZSI6IkRpZ2l0YWwgSWRlbnRpdHkgQ291cnNlIiwiZGVzY3JpcHRpb24iOiJBIGNvdXJzZSBvbiBEaWdpdGFsIGlkZW50aXR5IiwiaWQiOiJ1cm46dXVpZDphMzVhOGU0Zi03Y2Q0LTQwZGEtYWYzMi00MDI4OWRjMTZkOTEiLCJ0eXBlIjpbIkFjaGlldmVtZW50Il19LCJpZCI6ImRpZDpwcmlzbTpkMjI1MGQ5ZWUwNjNjM2Y1YmFlZDIxMmVkNDVjOWY1M2MwOGJjMTg5YzA0N2E5NWQ4NmRhNTBmZmRlZjQzY2VlOkNuc0tlUkk2Q2daaGRYUm9MVEVRQkVvdUNnbHpaV053TWpVMmF6RVNJUU9mTkFldHZBdVpCRnpXNFZ0akJzLUwyWENieXZIYXllMlZrU1ZwOUY1b1FCSTdDZ2R0WVhOMFpYSXdFQUZLTGdvSmMyVmpjREkxTm1zeEVpRUN2YVdOcXZzenZ3blVSdVd5eE9XM0dvMkxxUDV6N2NLTlZqV001TFk1bjM4IiwidHlwZSI6WyJBY2hpZXZlbWVudFN1YmplY3QiXX0sInR5cGUiOlsiVmVyaWZpYWJsZUNyZWRlbnRpYWwiXSwiQGNvbnRleHQiOlsiaHR0cHM6XC9cL3d3dy53My5vcmdcLzIwMThcL2NyZWRlbnRpYWxzXC92MSJdLCJjcmVkZW50aWFsU3RhdHVzIjp7InN0YXR1c1B1cnBvc2UiOiJSZXZvY2F0aW9uIiwic3RhdHVzTGlzdEluZGV4Ijo1LCJpZCI6Imh0dHA6XC9cLzEwLjEwLjUwLjEwNTo4MDAwXC9jbG91ZC1hZ2VudFwvY3JlZGVudGlhbC1zdGF0dXNcL2I5YjZiYjFlLTY4NjQtNDA3NC1iOGFjLTEyYjNhMGIzMGYwYyM1IiwidHlwZSI6IlN0YXR1c0xpc3QyMDIxRW50cnkiLCJzdGF0dXNMaXN0Q3JlZGVudGlhbCI6Imh0dHA6XC9cLzEwLjEwLjUwLjEwNTo4MDAwXC9jbG91ZC1hZ2VudFwvY3JlZGVudGlhbC1zdGF0dXNcL2I5YjZiYjFlLTY4NjQtNDA3NC1iOGFjLTEyYjNhMGIzMGYwYyJ9fX0.RaXJtH8DXmHEEDpJMrbT8e-AxsjH4zqXeGHIxT0Ly13zzJ0GStlFkdw_SLiggQI_7iyl9xupf2icaPURcOkouw
                                      """;

    private const string RevokedJwt = """
                                      eyJhbGciOiJFUzI1NksifQ.eyJpc3MiOiJkaWQ6cHJpc206NTM5MDBiN2MxZjFjNzA0NGVkNDk4OWFiNDY1NzBkMjkyNzIzNmU5NDE0ZmViZTgyZWEzNmFhYTkxN2E2NDJkZDpDb1FCQ29FQkVrSUtEbTE1TFdsemMzVnBibWN0YTJWNUVBSktMZ29KYzJWamNESTFObXN4RWlFQ2ZkNmlDYnp2TENTT05lbG12czNvUzJJWXl1ZzhaM2hwOU1aZVMyVzJCcmtTT3dvSGJXRnpkR1Z5TUJBQlNpNEtDWE5sWTNBeU5UWnJNUkloQWt5WkVjQ0FhTC1WZFBuUU9PdHVsVjZEU0k2eGIxVVNXRXhvUWxJbmwybWEiLCJzdWIiOiJkaWQ6cHJpc206ZDIyNTBkOWVlMDYzYzNmNWJhZWQyMTJlZDQ1YzlmNTNjMDhiYzE4OWMwNDdhOTVkODZkYTUwZmZkZWY0M2NlZTpDbnNLZVJJNkNnWmhkWFJvTFRFUUJFb3VDZ2x6WldOd01qVTJhekVTSVFPZk5BZXR2QXVaQkZ6VzRWdGpCcy1MMlhDYnl2SGF5ZTJWa1NWcDlGNW9RQkk3Q2dkdFlYTjBaWEl3RUFGS0xnb0pjMlZqY0RJMU5tc3hFaUVDdmFXTnF2c3p2d25VUnVXeXhPVzNHbzJMcVA1ejdjS05WaldNNUxZNW4zOCIsIm5iZiI6MTcyNjg0MzE5NiwiZXhwIjoyMDI2ODQzMTk2LCJ2YyI6eyJjcmVkZW50aWFsU3ViamVjdCI6eyJhY2hpZXZlbWVudCI6eyJhY2hpZXZlbWVudFR5cGUiOiJEaXBsb21hIiwiaW1hZ2UiOnsiaWQiOiJcLzlqXC80QUFRU2taSlJnQUJBUUFBQVFBQkFBRFwvMndDRUFBa0dCd2dIQmdrSUJ3Z0tDZ2tMRFJZUERRd01EUnNVRlJBV0lCMGlJaUFkSHg4a0tEUXNKQ1l4Sng4ZkxUMHRNVFUzT2pvNkl5c1wvUkQ4NFF6UTVPamNCQ2dvS0RRd05HZzhQR2pjbEh5VTNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTnpjM056YzNOemMzTlwvXC9BQUJFSUFKUUExZ01CSWdBQ0VRRURFUUhcL3hBQWJBQUVBQWdNQkFRQUFBQUFBQUFBQUFBQUFCZ2NCQkFVREF2XC9FQUVFUUFBRURBd0lEQlFVRUJ3WUhBQUFBQUFFQUFnTUVCUkVHSVFjU01STWlRVkZoRkRKeGdaRVZGa0toSTFKaWNxS3p3U1NDa3JHeThEWkVWWU9UMGRMXC94QUFZQVFFQUF3RUFBQUFBQUFBQUFBQUFBQUFBQVFJREJQXC9FQUNZUkFRQUNBUU1FQVFRREFBQUFBQUFBQUFBQkFoRURJVEVTUVdHaFVTSXlnZkFUSTNIXC8yZ0FNQXdFQUFoRURFUUFcL0FMeFJFUUVSRUJFUkFSRVFFUkVCRVJBUkVRRVJFQkVSQVJFUUVSRUJFUkFSRVFFUkVCRVJBUkVRRVJFQkVSQVJFUUZqSzhwNTRxYU4wczhySW8yN2w4amcxb0h4S2hWNDRvV1dsZjdOYUJOZDZ3N01qbzJGelNmM3ZINVpVMXJOdUVUYUk1VG9sYytHK1d5ZTRQdDhOZlRQcTJETG9XeUF1SHlWYVhTVFYxOHBuVk9wYnJCcFd6SHJEemhzcng2bjNzK21SOEZId3poek04VVZOVTNHaW1adkhkbk9kM25lWkhnUFhBV3NhWHl6blUrRjlnN0lxcW83bHJmVGNEWm8zeGFycytNc25weUhTdGJcL0FIZHpcL0Y4Vkk3SHhKMDlkWE5obHF2cytxSjVURFdqczlcL0lPNkVcL05WblRtT0ZvdkVwbWkrSTNpUm9jeHdjMDlDRGtGZmF6WEVSRUJFUkFSRVFFUkVCRVJBUkVRRVJFQkVSQVJFUWNEVitxS0hTdEhGVlhCazhnbWYyY1VjTGN1YzdCT1BMb0ZFVHF2VytvTXRzR25tVzJuUFNxcjNuT1BQQkF4K2E5ZU5lUlJhZGQ1WGVQXC9BRXVVSTR0M0s0SFZ0VlFPcjZvVWNjY1paVHRsTFk5MmduTFJzZFwvTmRHbHB4TVIrV043VERldkZGYVk1VE5ydlYwMTJuYnY3QlFIdWowUCsyclVqMXRVeE1OSm9uVHNWdWpjNFJDVU03U1Z4UFFFOUd1T0R0djhBRlErMFdxcHZGVzZqb25Sc2tFVDVBWkRodmRibGQrc3JLU2hpRWRuYUh3MWdZV1d5YnRXelU4d2FHOXJsdlZ4T2NEUGl0K21PSjNaUmFYQXZGVGNhcTRTbThUelMxYkhGcnUxZHpGaDhRUEFmSmR1NVZPbkg2UXBJS09rQXV6SEIwdUpua041dXBHZmVQZEhkSndPYnhVcjB0d25mVVFNcXRUVkVrYjVBSGV5d3VITUIrMjd6K0gxS2xqdUdXbERHWVwvWXBRUVBlRlM3bUhyMVZiYTFJMldycDJVdnBxdHZsTFg4bW5KcWdWTGdYZGpDN1orTnpscDJLa3NtdEtDN01aRHJYVGtWUUpHNUZiVERzcGNkT1lEeEd4M0JId1hSMVR3dXE3VTM3UTAxTkxWTmlQTzZuZTdFZ0E2OHJoMTI4Tmo1RlI4T3BMN1NPTHl6dG5NRkpRVytuNVwvN0E0dUI1M0YzNER2bnJqSkhWV3pTKzhLNHRYWkk3SlFGaEVuRFwvQUZweXQ2aTNYQTRQd0dkdjRmbXU3OSt0VDJJOG1xdE1TU1F0OTZyb0haSCtIcDg4aFUxY2FLUzNYQ3BvNXkxMHRQSTVqaXc3WkcyUVZPdUdsMHVOUlRhaG82aXZxWjZXSzJ2ZXlLYVF2RERuRzJlbTNnTmxXK25HTThwcmVjNFhIcHU5VW1vYlZGYzZBdjhBWjVTNE5EMjRjQzF4YVFmbUN1b29Sd1lHT0hkdFBtK2YrYzlUZGNsNHhhWWgweHdJaUtxUkVSQVJFUUVSRUJFUkFSRVFFUkVCRVJCWEhHM2EwV04zbGRvdjlEMVhcL0Z3WTExVmVaZ2hQOEtzRGpsdHArMU9cL1Z1Y1pcL2dlb0Z4Z1wvNDNuOWFhRVwva1YyYUhiOHVmVjd0S3l3UTAxcmdqdTlGV3dOcjUyelVWenBHYzhqUTBZZXdBWnowR3g4enRzcEpvRVVsWmY3bHF1XC8xampUVUx1U21tcm5ORHVZNXdYWUFITUc3QUFkU2R0bHE2WnFLYWxwTFkyaTFUVVVzTDRaRE5SdGlENW81ajFiRnQ0a2tqUDYzN1MzZUhsTGI3aG91OFVkMHBSTVk2NXZNMTdNdllYRm96NWc5UVQ4Vk41MmxGWTNoNzZyNHRudFBZOU1SdHk3YjJ1ZHVTZjNHZVB4UDBVRml1R3FhZTR1dXpYWGR0VWZlcUhReVlJOWNqR1BUb3BKWFdyVTFIZUsrMDZTWlVSVU5QTVdCMEhKRzkyMmU4ODRjN3I1cnhHaytJelhkb0RkTWpmUDJwdjhBekZOT2lzYkl0MVRPNlI2VDR1UVZCYlM2bWpaRklOaFZ3TlBJZjNtXC9oUG1SdDhGek5XVTFKYTlXeDExc3JhcUsxWDJGM2F1dGpXeU9jN3h3M0J6dVFTQU05VjUyT3pYdTc2Z3ByVHJPS2FTbWN4MGdmSVdHWExjWUFrSGV3ZlVyc2NRNldodGx5MHJiN1pFK2piRk8rVWV3eFwvcEkyN1pjQmpjN0U3NTZLbUlpXC93QksyODF6S0FYNkp0UmJhU3J0MXVtaG9LWE5OSldTN09xWmk0a3VQbWR2a2Nqd1haNFk3UTZwZityYWpcL21mXC9TODlYVFU5VGJLbVNvMUlibE8ydFBzbFBFd05hQVFPWjdcL1hjK21cLzdTOU9HMjF0MWNmSzFcLzhBMHRKbit0U0l4ZFp2QnNZNGMycjk2b1wvbnlLYUtHOElCamgzYWYrOVwvT2Vwa3VQVSsrWFRUN1lFUkZSWVJFUUVSRUJFUkFSRVFFUkVCRVJBUllkMFVPMUh4RHRXbTdrNmd1ZE5YaVVNRHc2T0VPYThIeEI1dlFoVEVUUENKbUk1YVhHU2dyTG5wMmppdDFKUFZTc3JtUExJWXk0Z0JydDl2RGRRdmlwWWJ4WDZ2ZFUwTnJyYWlFMDBUZTBpZ2M1dVJuSXlBdXRyVzRYTzk2cHNGSlk3clUyNks1MGJYczd4QUJMbkhKQVBYQXdvdXo3ME9iWjNEVXRVUHRTc2RTc1wvU3ZcL1JscitUSjMzQzZ0T0ppSTNZWDNtV3haNHRRV3VocDNVZWhwRGRxTnhkRlhleWNybnQ4bmRNbjZcLzFYUmlzZHcwXC9xWXd3VVYycXJWWHhRKzN6eFFPTHUxRCtjdkczNjdjNDhubjBYS3ZzT3E3SEJVVFZXbzZxUVExemFRY2tyK1wvelJoNGVOK20rTWVpeFdSYW9vNnlcLzBoMU5WU1NXV25FOGdiSzhkczNBSjVkOXNBaFdtTVwvQ0kyN0x3cUtDanJUelZWSkZQa2JkcEVDZnpXdjhBZCswZjlMcFBcL0VGVGNWTnFaMTVmYkpOVjFNTDRxQVZzMHBlOHRqYnk4M0xqT1NRTUZhMXFxYjFlTHBOUjBHczU1S2VLamZWT3JNU2h2SzBnT0hLZTluY0xMK0x5dkdwNFhwRlIwbHZoZStucEk0bWhwSkVNVzVId0hWVmZQSGZyMWZicnFSOXN1TkpWVUxHR3p3U1U3Z0g0THNodzgzQUg0Y3k0aktUVlVtb1lMUEZxZWQ0bm9oV3NxUkxKeW1JNTM1ZmV6c2RsNVVadjlkZWFlM1VPc0phbGxSVE9xSTZsa2p3TU5hWFljek9XdTJ4aFdycDQzeWliNTdQbStXMjhWdEpGQkZvbVdscWk4eTFOWEhTbHo1WEVrNERobmJmeitRWFQwSFlieFIyalZiYXExMXNMNmkzQmtEWklYTk1qdVwvc050ejBYTnNzdDV1ZG1xTG83VzFSU3NwY2UwUlBiSzkwZk03RGR3Y0hQb3ZxMHhhcHVsRmI2dURVMVUyQ3JmTEhLWFN2XC9BTEtZMjg1NXQ5OGdFXC9KYVRuR0ZJNXl0amhqUzFGQm9lMTB0WkJKVHpzYkp6eFN0TFhOekk0N2dcL0ZTcFZEdzUxWSszYWV2VjB2bFpXVjBGUFVSc2E0bm5jQWNqWUUrYW0ybGRhMEdxcDZpTzEwOVlHMDdRWkpKb2cxb3lkaG5KMzJKK1M1ZFNsdXFaYjBtTVJDVUlzRG9GbFpyaUlpQWlJZ0lpSUNJaUFpSWdJaUlNSG9xNzQwV1duck5OT3VaTEk2aWhJTFhFYnZhNGhwYitlUjhGWWg2S0I4VnJWZXI5YTZhM1dhbEVzWm01NXlYaHV6UnNOXC9WWDA5cnhLdDQrbVVIdkYxaHN0KzBYY3FsaGZGVDJ5TXVEZXVNdUczMVhPcWRSMmVDcTA3RlNUenpVOXRyM1ZVMHo0K1FrT2w1OEJ1ZW9Da3I0dGFVVnJpRmJwbXpWRU5GQUdDU1p2TzRNYlwvZStLK0x0cUJzV2o3UGY2Q3hXYVJ0Um1Lc2ErbHlJcFI1WVBUSVA1THBqR3pEZjVSM1V1cnFPODZhaG9jeWUxeFhKMDNNVzdPaEJkeUhPZW9EZ01laSsyNnF0UjRpWEM3eUdVMm12Z2ZUemQzdjhqbzJqM2NcL3JOSHlYMk5aWEEwbnRZMHZZdlo4T1BQN0x0aHZ2Zml6c3ZXWFU5NGhZOTh1azdHeGpHOHpuR21HQVA4ZlhZN2RkbGJwMnhqMnJudm4wMXJmcXloaTF0ZGJ2TFBORFRWTlBKRFR5TWpEbnN5ME5ZY2VtTXJYczJvNlMxNm11RnlrdU5UV21lM1N4TnFud05hOTBydVRHV2RNRGxXKzdWVjJaSExMSnBPeXRqaEFNcmpTYk1CYnpEUGU4UnV2b2FudTdobHVrN0lmMFlreDdMdnlrWkczTm5KOHVxbkhqMlpcL2NOQ3QxRmE3anFpZ3VsVFVWc0hMYm80NXA2YnVTTXFXODNmQUhWdTQyR01yWkdxTEpGclNudThZZnlOb254VlU3SVF3MUV6b3kzbjVBZHNuR1Y2MU9xcnJTeHVrcU5LMktOalMxcmlhWWJGM1FiUDhBRmVuM2l2ZWNmZEd5QThcL1pnR2xBeTdPTWJ2OEFOUjArUFpuejZSbXpYZWtvdEwzeTNUdWQyMWFJdXl3M2J1dXljbk95OWJGcUJsdjB0ZjdXNlo0ZlhzajdBTkdSbkpEeVQ0WmJnTHRWZXJybFJOYStyMHZZb211SkRTNm02a0VnOUhlWVAwWGNudnBwdEUwVjFxTEJhUHRHNVZnaXBJRzAzZExOOHVJem5vUFB4Q20wNDVqbnlSR2VcL3BGcks3UERUVVp6XC93QTNUNXdyajRkMlNuc21tS1JrQlkrU2Rvbm1sYitOemhuOHVpZ3QydFd0N25hSnJhTk8ycWxwNTNOYzgwMkdPUEtjajhTbmZEeW11bHYwelRVRjZwK3lucHlZMjRjSFpZRDNkeDZMRFduTVR2M2E2Y2JwT09peWc2SXVkc0lpSUNJaUFpSWdJaUlDSWlBaUlnRll3c29nK0h0RGdXdUFMU01FSHhDcDJhaGcwdmVhXC9URjdhZnUxZTNGMUxPVHRUeVp5M2Z3TFRnWjlHbnp4Y25pdVRxV3dVT283WExiN2pIelJ1M2E0ZTh4dzZFSFwvQUhsWHBicFwveFcwWlVEZmFXN2FScUpMUFVSeFBwbmN6bVBmRUhNcVdPMnpueDhzZFI4TUw1WmQzbHZNNitRdGM5cDUyXC9acmozem5tY2NEZHg1blpQamtxWVZcL3RPbVlmdVwvcnFsZGN0UHlPeFNYS01FdmdQZ00rQjlQOEFQb0k1ZTlCMWRQU0M1YWZuRjV0YnQyeTA0ekl3ZnROSDlQb0YyVnRXZVhOTVREVG11ejU0WllwZFJ0YzJhUHM1QjludkhPM2ZBT0I0WitXQWh1bVdNQnZzVHVSZ2EwbTJ2eUNCZ1A2ZThCNCtwWERvcElZNWlhaU5zakN4N1Mxd0p3ZVU0STlRY0xzbXVzaHJTNFVVZllGelNlNzA3c21mRG9DWThkM3dQeFduVGpoVm1xdUxLcGtyWmI1Qmlia01tTGRKbDNKMFgxVTM2cTdOMGpiNktpWnNnbFlEU1NOZHpaemtPT3crYTREbTl0VkZsS3h6akpJUkRFMEV1eG5ab0E2bkNtZHQwTEhRVWpMcHJXckZzb3M1YlRBXC9wNXYyY2VIeTMrQ2kzVFhrak10ZlRscnI5WXl0TnpsYkJhS0l1a3FheDNjRFFTWEVBNTZra24wK2ltZWw0ZnZycTJLN3gwNWcwN1ptOWxRTUxlVVNPSFREVDlmOEk2NVdyUVVOdzE3SERTVytsTmwwZEFSeU5hTVBxY2VQcm42RHpKNldyYkxmUzJ5aWhvNktKc1VFVFExakcrQVwvcWZWYzJycU42VmJRSG1zNFdVWE0yRVJFQkVSQVJFUUVSRUJFUkFSRVFFUkVCRVJBUkVRZU5WVFFWVk8rbnFZWTVvWGpsZEc5b0xYRHlJVmVYSGg1VzJpcWZjZEIzSjl2bk83cVNWeE1NbnA0XC9uOVFySlhtWCtqdm9yVnROVVRFU3BXOVYxdW5tTVhFVFRVMXRyRHQ5cDBUTm5lcDVldnk1bG9PMGZwbWtiOW9WMnNLZDlwUHVkaXpNNzhmaHdNN1wvQVorQ3ZHcmlncTRYUTFOT0pZbkRCWkl3T0IrU2k5Tnc3MHJUWEoxZkhhUVhuZHNUM09kRTArWWFUaitua3RxNjBZN3NwMDVtVU1zdGJVUGFZT0cybURDQ09VM1d1YU9Zanp5ZkQwejhsSnJKdzNpTldMbnF5c2t2VnhQVVNrOWszMEFQVWVtQVBSVG1Ia2pqREk0aXhqZGcxcmNBTDFEXC9SMzBWSjFabmhhS1k1Wll4ckdocldoclIwQUdBRjlJaXlhQ0lpQWlJZ0lpSUNJaUFpSWdJaUlDSWlBaUlnSWlJQ0lpQWlJZ3dtNnlpREN5aUlDSWlBaUlnSWlJQ0lpQWlJZ0lpSUNJaUFpSWdJaUlDSWlBaUlnSWlJQ0lpQWlJZ0lpSUNJaUFpSWdJaUlDSWlBaUlnXC9cL1oiLCJ0eXBlIjoiSW1hZ2UifSwiY3JpdGVyaWEiOnsibmFycmF0aXZlIjoiVGhyb3VnaCBzZXJpZXMgb2YgbGVzc29ucy5BbGlxdWFtIGVyYXQgdm9sdXRwYXQuIERvbmVjIGltcGVyZGlldCBlcm9zIHNhcGllbiwgZWdldCBwaGFyZXRyYSBzYXBpZW4gdnVscHV0YXRlIHV0LiBEdWlzIGlkIHZvbHV0cGF0IGRvbG9yLiBQcm9pbiB0aW5jaWR1bnQgbWF4aW11cyBibGFuZGl0LiBOYW0gaWQgbWFsZXN1YWRhIGVyYXQsIHNpdCBhbWV0IHNvZGFsZXMgbGVjdHVzLiBGdXNjZSB1dCBjb25zZXF1YXQgcHVydXMuIFV0IGludGVyZHVtIHNhcGllbiBldCB0b3J0b3IgbGFvcmVldCBwdWx2aW5hci4iLCJ0eXBlIjoiQ3JpdGVyaWEifSwibmFtZSI6IkRpZ2l0YWwgSWRlbnRpdHkgQ291cnNlIiwiZGVzY3JpcHRpb24iOiJBIGNvdXJzZSBvbiBEaWdpdGFsIGlkZW50aXR5IiwiaWQiOiJ1cm46dXVpZDphMzVhOGU0Zi03Y2Q0LTQwZGEtYWYzMi00MDI4OWRjMTZkOTEiLCJ0eXBlIjpbIkFjaGlldmVtZW50Il19LCJpZCI6ImRpZDpwcmlzbTpkMjI1MGQ5ZWUwNjNjM2Y1YmFlZDIxMmVkNDVjOWY1M2MwOGJjMTg5YzA0N2E5NWQ4NmRhNTBmZmRlZjQzY2VlOkNuc0tlUkk2Q2daaGRYUm9MVEVRQkVvdUNnbHpaV053TWpVMmF6RVNJUU9mTkFldHZBdVpCRnpXNFZ0akJzLUwyWENieXZIYXllMlZrU1ZwOUY1b1FCSTdDZ2R0WVhOMFpYSXdFQUZLTGdvSmMyVmpjREkxTm1zeEVpRUN2YVdOcXZzenZ3blVSdVd5eE9XM0dvMkxxUDV6N2NLTlZqV001TFk1bjM4IiwidHlwZSI6WyJBY2hpZXZlbWVudFN1YmplY3QiXX0sInR5cGUiOlsiVmVyaWZpYWJsZUNyZWRlbnRpYWwiXSwiQGNvbnRleHQiOlsiaHR0cHM6XC9cL3d3dy53My5vcmdcLzIwMThcL2NyZWRlbnRpYWxzXC92MSJdLCJjcmVkZW50aWFsU3RhdHVzIjp7InN0YXR1c1B1cnBvc2UiOiJSZXZvY2F0aW9uIiwic3RhdHVzTGlzdEluZGV4Ijo1LCJpZCI6Imh0dHA6XC9cLzEwLjEwLjUwLjEwNTo4MDAwXC9jbG91ZC1hZ2VudFwvY3JlZGVudGlhbC1zdGF0dXNcL2I5YjZiYjFlLTY4NjQtNDA3NC1iOGFjLTEyYjNhMGIzMGYwYyM1IiwidHlwZSI6IlN0YXR1c0xpc3QyMDIxRW50cnkiLCJzdGF0dXNMaXN0Q3JlZGVudGlhbCI6Imh0dHA6XC9cLzEwLjEwLjUwLjEwNTo4MDAwXC9jbG91ZC1hZ2VudFwvY3JlZGVudGlhbC1zdGF0dXNcL2I5YjZiYjFlLTY4NjQtNDA3NC1iOGFjLTEyYjNhMGIzMGYwYyJ9fX0.RaXJtH8DXmHEEDpJMrbT8e-AxsjH4zqXeGHIxT0Ly13zzJ0GStlFkdw_SLiggQI_7iyl9xupf2icaPURcOkouw
                                      """;

    public VerifyW3CCredentialTests()
    {
        _httpClient = new HttpClient();
        _credentialParser = new CredentialParser();

        var signatureHandler = new CheckSignatureHandler(new ExtractPrismPubKeyFromLongFormDid(), new EcServiceBouncyCastle());
        var expiryHandler = new CheckExpiryHandler();
        var revocationHandler = new CheckRevocationHandler(_httpClient);

        var serviceProvider = new ServiceCollection()
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CheckSignatureHandler).Assembly))
            .AddScoped<IRequestHandler<CheckSignatureRequest, Result<bool>>, CheckSignatureHandler>(_ => signatureHandler)
            .AddScoped<IRequestHandler<CheckExpiryRequest, Result<bool>>, CheckExpiryHandler>(_ => expiryHandler)
            .AddScoped<IRequestHandler<CheckRevocationRequest, Result<bool>>, CheckRevocationHandler>(_ => revocationHandler)
            .BuildServiceProvider();

        _mediator = serviceProvider.GetRequiredService<IMediator>();
        _handler = new VerifyW3CCredentialHandler(_mediator, _credentialParser);
    }

    [Fact]
    public async Task Handle_ValidCredential_ShouldReturnValidResult()
    {
        // Arrange
        var request = new VerifyW3CCredentialRequest(ValidJwt, true, true, false, false, false);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SignatureValid.Should().BeTrue();
        result.Value.IsExpired.Should().BeFalse();
        result.Value.IsRevoked.Should().BeFalse();
        result.Value.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExpiredCredential_ShouldReturnInvalidResult()
    {
        // Arrange
        var request = new VerifyW3CCredentialRequest(ExpiredJwt, true, true, false, false, false);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsExpired.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RevokedCredential_ShouldReturnInvalidResult()
    {
        // Arrange
        var request = new VerifyW3CCredentialRequest(RevokedJwt, true, false, true, false, false);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsRevoked.Should().BeTrue();
        result.Value.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_InvalidCredentialFormat_ShouldReturnFailure()
    {
        // Arrange
        const string invalidFormat = "invalid_format";
        var request = new VerifyW3CCredentialRequest(invalidFormat, true, false, false, false, false);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Failed to parse credential"));
    }

    [Fact]
    public async Task Handle_NullCredential_ShouldReturnFailure()
    {
        // Arrange
        var request = new VerifyW3CCredentialRequest(null, true, false, false, false, false);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Failed to parse credential"));
    }
}