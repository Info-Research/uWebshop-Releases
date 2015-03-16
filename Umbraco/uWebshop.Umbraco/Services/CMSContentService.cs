﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using umbraco;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.interfaces;
using umbraco.NodeFactory;
using uWebshop.Domain;
using uWebshop.Domain.Businesslogic;
using uWebshop.Domain.Interfaces;
using uWebshop.Umbraco.Businesslogic;
using Umbraco.Core.Models;
using File = uWebshop.Domain.File;
using Log = uWebshop.Domain.Log;
using Property = Umbraco.Core.Models.Property;

namespace uWebshop.Umbraco.Services
{
	internal class CMSContentService : ICMSContentService
	{
		public IUwebshopReadonlyContent GetReadonlyById(int id)
		{
			// might want to do some caching here
			return new NodeBasedContent(id);
		}

		public IEnumerable<IUwebshopReadonlyContent> GetAllRootNodes()
		{
			return new Node(-1).Children.Cast<INode>().Select(n => new NodeBasedContent(n.Id));
		}

		// todo: this functionality might be a double of the Store url functionality
		public string GenerateDomainUrlForContent(int id = 0)
		{
			try
			{
				if (id == 0) id = Node.GetCurrent().Id;
			}
			catch (Exception)
			{
				// intentional empty catch
			}

			string baseUrl = string.Empty;

			string http = "http://";
			if (HttpContext.Current.Request.IsSecureConnection)
			{
				http = "https://";
			}

			if (false) //id != 0)
			{
				if (library.GetCurrentDomains(id) != null && library.GetCurrentDomains(id).Any())
				{
					umbraco.cms.businesslogic.web.Domain firstOrDefaultDomain = library.GetCurrentDomains(id).FirstOrDefault();

					if (firstOrDefaultDomain != null && string.IsNullOrEmpty(firstOrDefaultDomain.Name))
					{
						baseUrl = string.Format("{0}{1}", http, firstOrDefaultDomain.Name);

						baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf("/", StringComparison.Ordinal));
					}
				}
			}
			else
			{
				string currentDomain = HttpContext.Current.Request.Url.Authority;
				baseUrl = string.Format("{0}{1}", http, currentDomain);
			}

			if (baseUrl == string.Empty || baseUrl == "http:/" || baseUrl == "https:/" || baseUrl == "http://" || baseUrl == "https://")
			{
				baseUrl = string.Format("{0}{1}", http, HttpContext.Current.Request.Url.Authority);
			}

			Log.Instance.LogDebug("baseUrl to return" + baseUrl);

			return baseUrl;
		}

		public IUwebshopContent GetById(int id)
		{
			return new DocumentBasedContent(id);
		}

		public File GetFileById(int id)
		{
			return InternalHelpers.LoadFileWithId(id);
		}

		public Image GetImageById(int id)
		{
			return InternalHelpers.LoadImageWithId(id);
		}


		private class DocumentBasedContent : IUwebshopContent
		{
			private readonly Content _content;

			public DocumentBasedContent(int id) : this(new Document(id))
			{
			}

			public DocumentBasedContent(Content document)
			{
				_content = document;
			}

			public string Path
			{
				get { return _content.Path; }
			}

			public DateTime CreateDate
			{
				get { return _content.CreateDate; }
			}

			public DateTime UpdateDate
			{
				get { return _content.UpdateDate; }
			}

			public int SortOrder
			{
				get { return _content.SortOrder; }
			}

			// todo: needed?
			public string UrlName
			{
				get { return _content.Name; }
			}

			IUwebshopReadonlyContent IUwebshopReadonlyContent.Parent
			{
				get { return new NodeBasedContent(_content.ParentId); }
			}

			public IUwebshopContent Parent
			{
				get { return new DocumentBasedContent(_content.ParentId); }
			}

			public int Id
			{
				get { return _content.Id; }
			}

			public string NodeTypeAlias
			{
				get { return _content.ContentType.Alias; }
			}

			public string Name
			{
				get { return _content.Name; }
			}

			public int template
			{
				get { return _content.Template.Id; }
			}

			public ICMSProperty GetProperty(string propertyAlias)
			{
				return new DocProperty(_content.Properties.FirstOrDefault(x => x.Alias == propertyAlias));
			}

			public ICMSProperty GetMultiStoreItem(string propertyAlias)
			{
				return new DocProperty(_content.GetMultiStoreItem(propertyAlias));
			}

			public string Url
			{
				get { throw new NotImplementedException(); }
			}

			public List<IUwebshopReadonlyContent> ChildrenAsList
			{
				get { throw new NotImplementedException(); }
			}

			public T1 GetProperty<T1>(string propertyAlias)
			{
				return _content.GetProperty<T1>(propertyAlias);
			}

			public void SetProperty(string propertyAlias, object value)
			{
				_content.SetProperty(propertyAlias, value);
			}

			public void Publish(int userId = 0)
			{
				_content.Publish(User.GetUser(userId));
			}

			public ICMSProperty getProperty(string propertyAlias)
			{
				return new DocProperty(_content.Properties.FirstOrDefault(x => x.Alias == propertyAlias));
			}
		}

		private class DocProperty : ICMSProperty
		{

			private readonly Umbraco.Core.Models.Property _property;

			public DocProperty(Umbraco.Core.Models.Property property)
			{
				this._property = property;
			}

			public string Value
			{
				get
				{
					if (_property != null && _property.Value != null)
						return _property.Value;
					return string.Empty;
				}
			}
		}

		private class NodeProperty : ICMSProperty
		{
			private readonly IProperty _property;

			public NodeProperty(IProperty property)
			{
				this._property = property;
			}

			public string Value
			{
				get
				{
					return _property != null ? _property.Value : string.Empty;
				}
			}
		}

		private class NodeBasedContent : IUwebshopReadonlyContent
		{
			private readonly INode _node;

			public NodeBasedContent(int id)
			{
				_node = new Node(id);
			}

			private NodeBasedContent(INode node)
			{
				_node = node;
			}

			public string Path
			{
				get { return _node.Path; }
			}

			public DateTime CreateDate
			{
				get { return _node.CreateDate; }
			}

			public DateTime UpdateDate
			{
				get { return _node.UpdateDate; }
			}

			public int SortOrder
			{
				get { return _node.SortOrder; }
			}

			public string UrlName
			{
				get { return _node.UrlName; }
			}

			public IUwebshopReadonlyContent Parent
			{
				get
				{
					if (_node.Parent == null) return null;
					return new NodeBasedContent(_node.Parent.Id);
				}
			}

			public int Id
			{
				get { return _node.Id; }
			}

			public string NodeTypeAlias
			{
				get { return _node.NodeTypeAlias; }
			}

			public string Name
			{
				get { return _node.Name; }
			}

			public int template
			{
				get { return _node.template; }
			}

			public ICMSProperty GetProperty(string propertyAlias)
			{
				return new NodeProperty(_node.GetProperty(propertyAlias));
			}

			public ICMSProperty GetMultiStoreItem(string propertyAlias)
			{
				return new NodeProperty(_node.GetMultiStoreItem(propertyAlias));
				
			}

			public string Url
			{
				get { return _node.Url; }
			}

			public List<IUwebshopReadonlyContent> ChildrenAsList
			{
				get { return _node.ChildrenAsList.Select(n => (IUwebshopReadonlyContent) new NodeBasedContent(n)).ToList(); }
			}
		}
	}
}
