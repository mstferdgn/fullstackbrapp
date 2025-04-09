import React, { useEffect, useState } from 'react';
import { PieChart, Pie, Cell, Tooltip, Legend } from 'recharts';
import { Typography } from 'antd';
import axios from 'axios';
import api from '../services/api';

const { Title } = Typography;

const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#A28BD4', '#FF6666'];

const AuthorReviewPie = () => {
  const [chartData, setChartData] = useState([]);

  useEffect(() => {
    const fetchData = async () => {
      try {
        // API çağrısını yapıyoruz.
        const response = await api.get('/authors/review-counts');
        // response.data şu şekilde: 
        // [{ name: 'Yazar 1', value: 12 }, { name: 'Yazar 2', value: 5 }, { name: 'Yazar 3', value: 20 }]
        setChartData(response.data);
      } catch (error) {
        console.error("Data fetch error:", error);
      }
    };

    fetchData();
}, []);

  return (
    <div style={{ textAlign: 'center', padding: '40px' }}>
      <Title level={3}>Yazar Bazlı İnceleme Dağılımı</Title>
      <PieChart width={600} height={500}>
        <Pie
          data={chartData}
          dataKey="value"
          nameKey="name"
          cx="50%"
          cy="50%"
          outerRadius={100}
          fill="#8884d8"
          label
        >
          {chartData.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
          ))}
        </Pie>
        <Tooltip />
        <Legend  itemGap={20}/>
      </PieChart>
    </div>
  );
};

export default AuthorReviewPie;
