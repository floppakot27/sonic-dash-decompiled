public class OfferContent
{
	public enum Bonus
	{
		Node,
		Character,
		Revives,
		Headstarts
	}

	public class Offer
	{
		public int m_offerID;

		public int m_frequencyCap;

		public int m_displayCap;

		public string m_productID;

		public string m_beforeProductID;

		public int m_percentOff;

		public bool m_nonPayingUsers;

		public Bonus m_bonusType;

		public int m_bonusCount;

		public Offer(int offerID, bool nonPayingUsers, string beforeProductID, string productID, int perecentOff, Bonus bonusType, int bonusCount, int frequencyCap, int displayCap)
		{
			m_offerID = offerID;
			m_nonPayingUsers = nonPayingUsers;
			m_beforeProductID = beforeProductID;
			m_productID = productID;
			m_percentOff = perecentOff;
			m_bonusType = bonusType;
			m_bonusCount = bonusCount;
			m_frequencyCap = frequencyCap;
			m_displayCap = displayCap;
		}
	}

	private static Offer[] s_offers = new Offer[9]
	{
		new Offer(1, nonPayingUsers: true, "sdBundle_Stars_11", "sdBundle_Stars_11_half_price", 50, Bonus.Character, 1, 48, 3),
		new Offer(2, nonPayingUsers: true, "sdBundle_Stars_11", "sdBundle_Stars_11_half_price", 50, Bonus.Revives, 1, 48, 3),
		new Offer(3, nonPayingUsers: true, "sdBundle_Stars_11", "sdBundle_Stars_11_half_price", 50, Bonus.Headstarts, 1, 48, 3),
		new Offer(4, nonPayingUsers: true, "sdBundle_Stars_11", "sdBundle_Stars_11_deep_cut", 75, Bonus.Character, 1, 48, 3),
		new Offer(5, nonPayingUsers: true, "sdBundle_Stars_11", "sdBundle_Stars_11_deep_cut", 75, Bonus.Revives, 1, 48, 3),
		new Offer(6, nonPayingUsers: true, "sdBundle_Stars_11", "sdBundle_Stars_11_deep_cut", 75, Bonus.Headstarts, 1, 48, 3),
		new Offer(7, nonPayingUsers: true, "sdBundle_Stars_11", "sdBundle_Stars_11_half_price", 50, Bonus.Character, 1, 24, 1),
		new Offer(8, nonPayingUsers: true, "sdBundle_Stars_11", "sdBundle_Stars_11_half_price", 50, Bonus.Revives, 1, 24, 1),
		new Offer(9, nonPayingUsers: true, "sdBundle_Stars_11", "sdBundle_Stars_11_half_price", 50, Bonus.Headstarts, 1, 24, 1)
	};

	public static Offer GetOffer(int offerID)
	{
		Offer[] array = s_offers;
		foreach (Offer offer in array)
		{
			if (offer.m_offerID == offerID)
			{
				return offer;
			}
		}
		return s_offers[0];
	}
}
