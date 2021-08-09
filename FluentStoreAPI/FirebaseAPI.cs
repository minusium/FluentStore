﻿using FluentStoreAPI.Models.Firebase;
using Flurl.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FluentStoreAPI
{
    public partial class FluentStoreAPI
    {
        private IFlurlRequest GetFirestoreBase()
        {
            return FIRESTORE_BASE_URL.WithTimeout(10);
        }

        private async Task<Document> GetDocument(bool authenticate, string documentId, params string[] bucket)
        {
            var request = GetFirestoreBase().AppendPathSegments(bucket).AppendPathSegment(documentId);
            if (authenticate)
                request = request.WithOAuthBearerToken(Token);

            var document = await request.GetJsonAsync<Newtonsoft.Json.Linq.JObject>();
            return document.ToObject<Document>();
        }

        private async Task<Document> GetUserDocument(string userId, string bucket, string documentId)
        {
            return await GetDocument(true, documentId, "users", userId, bucket);
        }

        private async Task<bool> UpdateUserDocument(string userId, string bucket, string documentId, Document doc)
        {
            var respose = await GetFirestoreBase().AppendPathSegments("users", userId, bucket, documentId)
                .WithOAuthBearerToken(Token).PatchJsonAsync(doc);
            return respose.ResponseMessage.IsSuccessStatusCode;
        }

        private async Task<List<Document>> GetUserBucket(string userId, string bucket)
        {
            var respose = await GetFirestoreBase().AppendPathSegments("users", userId, bucket)
                .WithOAuthBearerToken(Token).GetJsonAsync<Newtonsoft.Json.Linq.JObject>();
            return respose["documents"].ToObject<List<Document>>();
        }
    }
}
