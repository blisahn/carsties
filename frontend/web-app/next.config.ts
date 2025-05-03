import type { NextConfig } from "next";
import withFlowbiteReact from "flowbite-react/plugin/nextjs";

const nextConfig: NextConfig = {
  /* config options here */
  logging: {
    fetches: {
      fullUrl: true
    }
  },
  images: {
    remotePatterns: [
      new URL('https://pixabay.com/photos/**'),
      new URL('https://cdn.pixabay.com/photo/**')
    ]
  }
};

export default withFlowbiteReact(nextConfig);